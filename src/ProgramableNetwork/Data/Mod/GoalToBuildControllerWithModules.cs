using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Messages.Goals;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using System.Linq;
using ProtoId = Mafi.Core.Prototypes.Proto.ID;

namespace ProgramableNetwork;

/// <summary>
/// Custom goal type for this mod's tutorial flow. Completes when the player adds a module of every required
/// <see cref="ModuleProto"/> to a single controller — used to verify the worked example (two constants + a
/// sum + a display) was built.
///
/// The goal links to the last-placed controller (the one the player just built for the preceding
/// "place a controller" goal) and tracks only that one. We subscribe to <see cref="IConstructionManager"/>
/// with <c>AddNonSaveable</c> (the same approach as Mafi's internal, non-reusable GoalEntityTracker): the
/// link is adopted when the first controller is constructed (or the highest-id existing one on reload), and
/// only re-points to the newest remaining controller if the linked one is deconstructed. Module composition
/// is then polled per tick over that single controller, since modules are added after construction and
/// there is no module-added event to hook.
///
/// Serialization mirrors the mod's Module contract: [ManuallyWrittenSerialization] + static delegate thunks.
/// The manager references are persisted alongside the base goal flags (as Mafi's own GoalToBuildStorage does)
/// rather than resolved in <see cref="initAfterLoad"/>: Mafi only logs an exception thrown out of an
/// [InitAfterLoad] method, so a resolver-only restore leaves the fields null and blows up on the next update.
/// The remaining live state ([DoNotSave]) is rebuilt / re-subscribed lazily. The base <see cref="Goal"/> calls
/// <see cref="Destroy"/> on completion, which unsubscribes — so the listeners live exactly "until the tutorial
/// is done".
/// </summary>
[ManuallyWrittenSerialization]
public class GoalToBuildControllerWithModules : Goal
{
    public class Proto : GoalProto
    {
        public readonly ImmutableArray<ModuleProto.ID> RequiredModules;
        public readonly LocStrFormatted TitleText;

        public override Type Implementation => typeof(GoalToBuildControllerWithModules);

        public Proto(string id, LocStrFormatted title, ImmutableArray<ModuleProto.ID> requiredModules,
            ProtoId? tutorial = null, int lockedByIndex = -1,
            TutorialUnlockMode tutorialUnlock = TutorialUnlockMode.DoNotUnlock)
            : base(id, tutorial, lockedByIndex, null, tutorialUnlock)
        {
            RequiredModules = requiredModules;
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
        (obj, writer) => ((GoalToBuildControllerWithModules)obj).SerializeData(writer);
    private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
        (obj, reader) => ((GoalToBuildControllerWithModules)obj).DeserializeData(reader);

    private EntitiesManager m_entitiesManager;
    private IConstructionManager m_constructionManager;
    [DoNotSave]
    private DependencyResolver m_resolver;
    [DoNotSave]
    private Proto m_goalProto;
    [DoNotSave]
    private Controller m_target;
    [DoNotSave]
    private Action<IStaticEntity> m_onConstructed;
    [DoNotSave]
    private Action<IStaticEntity> m_onDeconstruction;
    [DoNotSave]
    private bool m_subscribed;
    [DoNotSave]
    private int m_lastCount;
    [DoNotSave]
    private bool m_loggedMissingRefs;

    public GoalToBuildControllerWithModules(Proto goalProto, EntitiesManager entitiesManager, IConstructionManager constructionManager)
        : base(goalProto)
    {
        m_goalProto = goalProto;
        m_entitiesManager = entitiesManager;
        m_constructionManager = constructionManager;
        updateTitle(0);
    }

    public static void Serialize(GoalToBuildControllerWithModules value, BlobWriter writer)
    {
        if (writer.TryStartClassSerialization(value))
        {
            writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
        }
    }

    public static GoalToBuildControllerWithModules Deserialize(BlobReader reader)
    {
        if (reader.TryStartClassDeserialization(out GoalToBuildControllerWithModules obj))
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

        int total = m_goalProto.RequiredModules.Length;
        int have = 0;
        if (m_target != null && m_target.IsConstructed)
        {
            foreach (ProtoId reqId in m_goalProto.RequiredModules)
            {
                if (m_target.Modules.AsEnumerable().Any(m => m.Prototype.Id == reqId))
                {
                    have++;
                }
            }
        }

        if (have != m_lastCount)
        {
            updateTitle(have);
        }

        return total > 0 && have >= total;
    }

    public override void Destroy()
    {
        if (m_subscribed)
        {
            m_constructionManager.EntityConstructed.RemoveNonSaveable(this, m_onConstructed);
            m_constructionManager.EntityStartedDeconstruction.RemoveNonSaveable(this, m_onDeconstruction);
            m_subscribed = false;
        }
        m_target = null;
    }

    protected override void UpdateTitleOnLoad()
    {
        // Runs at InitPriority.Lowest, i.e. after initAfterLoad — but only if that one managed to restore
        // the proto. Skip rather than throw out of the init hook.
        if (m_goalProto == null)
        {
            return;
        }
        updateTitle(m_lastCount);
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

    // Links to the last-placed controller and subscribes to construction events with AddNonSaveable, so the
    // listeners are not serialized and re-attach on the first update after load. Idempotent.
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

        // On a fresh activation there is usually no controller yet (the preceding goal isn't done); on a
        // reload this re-links to the still-existing last-placed controller.
        m_target = pickLastPlaced(null);

        m_onConstructed = e =>
        {
            // Adopt the freshly placed controller only when we have no live link — i.e. lock onto the one
            // built for the "place a controller" goal and keep it, rather than chasing later controllers.
            if (e is Controller controller && (m_target == null || !m_target.IsConstructed))
            {
                m_target = controller;
                updateTitle(m_lastCount);
            }
        };
        m_onDeconstruction = e =>
        {
            if (e is Controller controller && controller == m_target)
            {
                m_target = pickLastPlaced(controller);
            }
        };
        m_constructionManager.EntityConstructed.AddNonSaveable(this, m_onConstructed);
        m_constructionManager.EntityStartedDeconstruction.AddNonSaveable(this, m_onDeconstruction);
        return true;
    }

    // Highest entity id among constructed controllers = most recently placed (ids are monotonic).
    private Controller pickLastPlaced(Controller exclude)
    {
        Controller best = null;
        foreach (IEntity entity in m_entitiesManager.Entities)
        {
            if (entity is Controller controller && controller != exclude && controller.IsConstructed
                && (best == null || controller.Id.Value > best.Id.Value))
            {
                best = controller;
            }
        }
        return best;
    }

    private void updateTitle(int have)
    {
        m_lastCount = have;
        int total = m_goalProto.RequiredModules.Length;
        Title = m_goalProto.TitleText.Value + $" ({have} / {total})";
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
        // m_subscribed defaults to false, so the listeners re-attach on the next UpdateInternal (if still active).
        tryRestoreRefs();
    }
}
