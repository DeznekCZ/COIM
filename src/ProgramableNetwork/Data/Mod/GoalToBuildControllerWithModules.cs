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
/// All live state ([DoNotSave]) is rebuilt in <see cref="initAfterLoad"/> / re-subscribed lazily; only the
/// base goal flags persist. The base <see cref="Goal"/> calls <see cref="Destroy"/> on completion, which
/// unsubscribes — so the listeners live exactly "until the tutorial is done".
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

    private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
        (obj, writer) => ((GoalToBuildControllerWithModules)obj).SerializeData(writer);
    private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
        (obj, reader) => ((GoalToBuildControllerWithModules)obj).DeserializeData(reader);

    [DoNotSave]
    private Proto m_goalProto;
    [DoNotSave]
    private EntitiesManager m_entitiesManager;
    [DoNotSave]
    private IConstructionManager m_constructionManager;
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
        ensureSubscribed();

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
        updateTitle(m_lastCount);
    }

    // Links to the last-placed controller and subscribes to construction events with AddNonSaveable, so the
    // listeners are not serialized and re-attach on the first update after load. Idempotent.
    private void ensureSubscribed()
    {
        if (m_subscribed)
        {
            return;
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
        // m_subscribed defaults to false, so the listeners re-attach on the next UpdateInternal (if still active).
        m_goalProto = (Proto)Prototype;
        m_entitiesManager = resolver.Resolve<EntitiesManager>();
        m_constructionManager = resolver.Resolve<IConstructionManager>();
    }
}
