using Mafi;
using Mafi.Collections;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Messages.Goals;
using Mafi.Core.Ports.Io;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using Mafi.Core.Ports;

namespace ProgramableNetwork;

/// <summary>
/// Custom goal type for this mod's tutorial flow. Completes when the player has a storage that stores a
/// product and is fed that product by a belt coming from a producing machine — i.e. a
/// "producer → belt → storage" chain. This sets up the kind of storage a controller's Connection module
/// can then monitor.
///
/// Verification (per tracked storage with an assigned product): for each connected <b>input</b> port, the
/// port must connect to a belt (<see cref="Transport"/>); we then walk that belt chain upstream
/// (StartInputPort → connected output) until the first non-belt entity. If that source is neither a belt nor
/// another storage, it's treated as the producing machine and the goal is satisfied. A direct port-to-port
/// machine→storage adjacency (no belt) does not count, matching "connected by belt".
///
/// Storages are tracked event-driven via <see cref="IConstructionManager"/> + <c>AddNonSaveable</c> (the
/// same approach as Mafi's internal, non-reusable GoalEntityTracker), so each tick only walks live storages.
/// Belts need no tracking — a belt added later just shows up on the storage's ports on the next walk.
///
/// Serialization mirrors the mod's Module contract: [ManuallyWrittenSerialization] + static delegate thunks.
/// The manager references are persisted alongside the base goal flags (as Mafi's own GoalToBuildStorage does)
/// rather than resolved in <see cref="initAfterLoad"/>: Mafi only logs an exception thrown out of an
/// [InitAfterLoad] method, so a resolver-only restore leaves the fields null and blows up on the next update.
/// The remaining live state ([DoNotSave]) is rebuilt / re-subscribed lazily. The base <see cref="Goal"/> calls
/// <see cref="Destroy"/> on completion, which unsubscribes the listeners.
/// </summary>
[ManuallyWrittenSerialization]
public class GoalToConnectStorageToProducer : Goal
{
    // Guards the upstream walk against a pathological cyclic belt layout.
    private const int MaxBeltHops = 256;

    public class Proto : GoalProto
    {
        public readonly LocStrFormatted TitleText;

        public override Type Implementation => typeof(GoalToConnectStorageToProducer);

        public Proto(string id, LocStrFormatted title, ID? tutorial = null, int lockedByIndex = -1,
            TutorialUnlockMode tutorialUnlock = TutorialUnlockMode.DoNotUnlock)
            : base(id, tutorial, lockedByIndex, null, tutorialUnlock)
        {
            TitleText = title;
        }
    }

    // Version 1: base goal flags only.
    // Version 2: the manager references are stored in the save (see SerializeData) instead of being
    //            re-resolved after load — Mafi only *logs* an exception thrown out of an [InitAfterLoad]
    //            method, so a purely resolver-based restore turns any hiccup there into null fields and a
    //            NullReferenceException on the next update. Mafi's own GoalToBuildStorage stores its
    //            EntitiesManager for the same reason.
    private const int VERSION_MANAGER_REFS = 2;

    private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
        (obj, writer) => ((GoalToConnectStorageToProducer)obj).SerializeData(writer);
    private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
        (obj, reader) => ((GoalToConnectStorageToProducer)obj).DeserializeData(reader);

    private EntitiesManager m_entitiesManager;
    private IConstructionManager m_constructionManager;
    [DoNotSave]
    private DependencyResolver m_resolver;
    [DoNotSave]
    private Proto m_goalProto;
    [DoNotSave]
    private Set<Storage> m_storages;
    [DoNotSave]
    private Action<IStaticEntity> m_onConstructed;
    [DoNotSave]
    private Action<IStaticEntity> m_onDeconstruction;
    [DoNotSave]
    private bool m_subscribed;
    [DoNotSave]
    private bool m_loggedMissingRefs;

    public GoalToConnectStorageToProducer(Proto goalProto, EntitiesManager entitiesManager, IConstructionManager constructionManager)
        : base(goalProto)
    {
        m_goalProto = goalProto;
        m_entitiesManager = entitiesManager;
        m_constructionManager = constructionManager;
        Title = goalProto.TitleText.Value;
    }

    public static void Serialize(GoalToConnectStorageToProducer value, BlobWriter writer)
    {
        if (writer.TryStartClassSerialization(value))
        {
            writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
        }
    }

    public static GoalToConnectStorageToProducer Deserialize(BlobReader reader)
    {
        if (reader.TryStartClassDeserialization(out GoalToConnectStorageToProducer obj))
        {
            reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
        }
        return obj;
    }

    protected override bool UpdateInternal()
    {
        if (!ensureSubscribed())
        {
            return false;
        }

        foreach (Storage storage in m_storages)
        {
            if (storage.IsConstructed && storage.StoredProduct.HasValue && isFedByProducerBelt(storage))
            {
                return true;
            }
        }
        return false;
    }

    public override void Destroy()
    {
        if (m_subscribed)
        {
            m_constructionManager.EntityConstructed.RemoveNonSaveable(this, m_onConstructed);
            m_constructionManager.EntityStartedDeconstruction.RemoveNonSaveable(this, m_onDeconstruction);
            m_subscribed = false;
        }
        m_storages?.Clear();
    }

    protected override void UpdateTitleOnLoad()
    {
        // Runs at InitPriority.Lowest, i.e. after initAfterLoad — but only if that one managed to restore
        // the proto. Skip rather than throw out of the init hook.
        if (m_goalProto == null)
        {
            return;
        }
        Title = m_goalProto.TitleText.Value;
    }

    private bool isFedByProducerBelt(Storage storage)
    {
        foreach (IoPort port in storage.Ports)
        {
            // Only inputs (the storage receiving) that connect to a belt count — a direct machine adjacency
            // (no belt) is rejected, matching "connected by belt".
            if (port.Type != IoPortType.Input || !port.IsConnected)
            {
                continue;
            }

            if (!(port.ConnectedPort.Value.OwnerEntity is Transport belt))
            {
                continue;
            }

            IEntityWithPorts source = walkBeltChainUpstream(belt);
            if (source != null && !(source is Transport) && !(source is Storage))
            {
                return true;
            }
        }
        return false;
    }

    // Follows a belt (and any belts chained before it) upstream to the first non-belt entity feeding it.
    private IEntityWithPorts walkBeltChainUpstream(Transport belt)
    {
        for (int hop = 0; belt != null && hop < MaxBeltHops; hop++)
        {
            Option<IoPort> upstream = belt.StartInputPort.ConnectedPort;
            if (!upstream.HasValue)
            {
                return null;
            }

            IEntityWithPorts owner = upstream.Value.OwnerEntity;
            if (owner is Transport previousBelt)
            {
                belt = previousBelt;
                continue;
            }
            return owner;
        }
        return null;
    }

    // Fills in references the post-load init hook could not restore. Mafi runs [InitAfterLoad] methods inside
    // a catch-all that only logs the failure (BlobReader.FinalizeLoadingTimeSliced), and DependencyResolver
    // refuses a Resolve while it is already inside an external call — so that hook can legitimately fail
    // part-way through and leave these null. Retrying here, on the sim thread, recovers such a save.
    private bool tryRestoreRefs()
    {
        if (m_goalProto == null)
        {
            m_goalProto = Prototype as Proto;
        }

        if (m_resolver != null && (m_entitiesManager == null || m_constructionManager == null))
        {
            try
            {
                if (m_entitiesManager == null)
                {
                    m_entitiesManager = m_resolver.Resolve<EntitiesManager>();
                }
                if (m_constructionManager == null)
                {
                    m_constructionManager = m_resolver.Resolve<IConstructionManager>();
                }
            }
            catch (Exception ex)
            {
                if (!m_loggedMissingRefs)
                {
                    m_loggedMissingRefs = true;
                    Log.Exception(ex, $"[Programable Network] Goal '{Prototype?.Id}' failed to resolve its managers.");
                }
            }
        }

        return m_goalProto != null && m_entitiesManager != null && m_constructionManager != null;
    }

    // Tracks constructed storages event-driven; listeners are AddNonSaveable so they aren't serialized and
    // re-attach on the first update after load. Idempotent.
    // Returns false when the goal has no live references to work with (a save whose post-load init did not
    // complete): the goal then simply never progresses instead of throwing on every update.
    private bool ensureSubscribed()
    {
        if (m_subscribed)
        {
            return true;
        }

        if (!tryRestoreRefs())
        {
            if (!m_loggedMissingRefs)
            {
                m_loggedMissingRefs = true;
                Log.Error($"[Programable Network] Goal '{Prototype?.Id}' has no live references after load "
                    + $"(proto: {m_goalProto != null}, entities: {m_entitiesManager != null}, "
                    + $"construction: {m_constructionManager != null}); it will not track progress.");
            }
            return false;
        }

        m_subscribed = true;

        m_storages = new Set<Storage>();
        foreach (IEntity entity in m_entitiesManager.Entities)
        {
            if (entity is Storage storage && storage.IsConstructed)
            {
                m_storages.Add(storage);
            }
        }

        m_onConstructed = e =>
        {
            if (e is Storage storage)
            {
                m_storages.Add(storage);
            }
        };
        m_onDeconstruction = e =>
        {
            if (e is Storage storage)
            {
                m_storages.Remove(storage);
            }
        };
        m_constructionManager.EntityConstructed.AddNonSaveable(this, m_onConstructed);
        m_constructionManager.EntityStartedDeconstruction.AddNonSaveable(this, m_onDeconstruction);
        return true;
    }

    protected override void SerializeData(BlobWriter writer)
    {
        base.SerializeData(writer);
        writer.WriteInt(/* Version */ VERSION_MANAGER_REFS);
        EntitiesManager.Serialize(m_entitiesManager, writer);
        writer.WriteGeneric(m_constructionManager);
    }

    protected override void DeserializeData(BlobReader reader)
    {
        base.DeserializeData(reader);
        int version = reader.ReadInt();
        if (version >= VERSION_MANAGER_REFS)
        {
            m_entitiesManager = EntitiesManager.Deserialize(reader);
            m_constructionManager = reader.ReadGenericAs<IConstructionManager>();
        }
        reader.RegisterInitAfterLoad(this, nameof(initAfterLoad), InitPriority.Normal);
    }

    [InitAfterLoad(InitPriority.Normal)]
    private void initAfterLoad(DependencyResolver resolver)
    {
        // Stash the resolver first, so tryRestoreRefs can retry later even if anything below fails.
        m_resolver = resolver;

        // Only version-1 saves reach here without their manager references; tryRestoreRefs does the rest and
        // is deliberately the single place that touches Prototype / Resolve, so both paths behave the same.
        tryRestoreRefs();
    }
}
