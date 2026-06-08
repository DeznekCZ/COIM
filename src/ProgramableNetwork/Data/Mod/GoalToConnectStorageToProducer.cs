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
/// Serialization mirrors the mod's Module contract: [ManuallyWrittenSerialization] + static delegate thunks;
/// live state ([DoNotSave]) is rebuilt in <see cref="initAfterLoad"/> / re-subscribed lazily. The base
/// <see cref="Goal"/> calls <see cref="Destroy"/> on completion, which unsubscribes the listeners.
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

    private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
        (obj, writer) => ((GoalToConnectStorageToProducer)obj).SerializeData(writer);
    private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
        (obj, reader) => ((GoalToConnectStorageToProducer)obj).DeserializeData(reader);

    [DoNotSave]
    private Proto m_goalProto;
    [DoNotSave]
    private EntitiesManager m_entitiesManager;
    [DoNotSave]
    private IConstructionManager m_constructionManager;
    [DoNotSave]
    private Set<Storage> m_storages;
    [DoNotSave]
    private Action<IStaticEntity> m_onConstructed;
    [DoNotSave]
    private Action<IStaticEntity> m_onDeconstruction;
    [DoNotSave]
    private bool m_subscribed;

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
        ensureSubscribed();

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

    // Tracks constructed storages event-driven; listeners are AddNonSaveable so they aren't serialized and
    // re-attach on the first update after load. Idempotent.
    private void ensureSubscribed()
    {
        if (m_subscribed)
        {
            return;
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
    }

    protected override void SerializeData(BlobWriter writer)
    {
        base.SerializeData(writer);
        writer.WriteInt(/* Version */ 1);
    }

    protected override void DeserializeData(BlobReader reader)
    {
        base.DeserializeData(reader);
        // Read (and currently ignore) the version stamp; gate future fields on it per the deserialization skill.
        reader.ReadInt();
        reader.RegisterInitAfterLoad(this, nameof(initAfterLoad), InitPriority.Normal);
    }

    [InitAfterLoad(InitPriority.Normal)]
    private void initAfterLoad(DependencyResolver resolver)
    {
        // Prototype is restored by base.DeserializeData; rebuild the non-saved live references from it.
        m_goalProto = (Proto)Prototype;
        m_entitiesManager = resolver.Resolve<EntitiesManager>();
        m_constructionManager = resolver.Resolve<IConstructionManager>();
    }
}
