using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Camera;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Display = Mafi.Unity.Ui.Library.Display;
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;

namespace ProgramableNetwork
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public partial class ControllerInspector : BaseInspector<Controller>, ISelectionInspector<IEntity, EntitySelector, Controller>
    {
        private readonly AudioSource m_invalidOpSound;
        private bool m_highlightSearched;
        private IRenderedEntity m_hoveredEntity;
        private readonly PanelWithHeader m_modulesPanel;
        private readonly LinesFactory m_linesFactory;
        private readonly List<LineMb> m_lines = new List<LineMb>();
        private readonly Material m_movingArrowsLineMaterialShared;
        private readonly ControllerView m_view;

        // TODO
        public ModuleConnector m_higlightedOutput;
        public ModuleConnector m_higlightedInput;
        public bool m_showsLinks;

        public ControllerInspector(
            UiContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager,
            //TerrainCursor terrainCursor,
            CameraController cameraController,
            NewInstanceOf<EntityHighlighter> entityHighlighter,
            NewInstanceOf<EntityHighlighter> entityHighlighterSelectable,
            LinesFactory linesFactory,
            AssetsDb assetsDb
            ) : base(context)
        {
            m_linesFactory = linesFactory;
            m_movingArrowsLineMaterialShared = assetsDb.GetSharedMaterial("Assets/Core/Materials/MovingArrowsLine.mat");
            CursorManager = cursorManager;
            CameraController = cameraController;
            CursorPickingManager = cursorPickingManager;
            //TerrainCursor = terrainCursor;
            EntityHighlighter = entityHighlighter.Instance;
            EntityHighlighterSelectable = entityHighlighterSelectable.Instance;
            ShortcutsManager = shortcutsManager;
            m_invalidOpSound = context.AudioDb.InvalidOp();

            ProgressBar bar;
            AddPanelRow(
                    new Label()
                        .Value("Computing speed".AsLoc())
                        .TextAlign(TextAlignment.LeftMiddle),
                    new UiComponent().Fill(),
                    bar = new ProgressBar()
                        .HeightAuto()
                        .Width(150),
                    new Display()
                        .Value(0)
                        .Width(150)
                        .Tooltip("Ticks per 60 seconds".AsLoc())
                        .ObserveValue(() => $"{(600 / (1f + Entity.Speed)).ToFix32().ToStringRounded(0)} t/m"),
                    new ButtonText("-".AsLoc())
                        .TextAlign(TextAlignment.CenterMiddle)
                        .Width(50)
                        .OnClick(() => Entity.Speed++)
                        .ObserveEnabled(() => Entity.Speed < 29),
                    new ButtonText("+".AsLoc())
                        .TextAlign(TextAlignment.CenterMiddle)
                        .Width(50)
                        .OnClick(() => Entity.Speed--)
                        .ObserveEnabled(() => Entity.Speed > 0)
                )
                .BodyGap(5.px());

            bar.ObserveVisibleForRender(() => Entity.Speed >= 10)
               .Observe(() => Entity.Speed)
               .Observe(() => Entity.Clock)
               .Observe(() => Entity.IsEnabled)
               .Do((speed, clock, enabled) =>
               {
                   bar.Color(enabled ? ColorRgba.GreenYellow : ColorRgba.DarkYellow);
                   if (speed == clock)
                       bar.Value(Percent.Hundred);
                   else
                       bar.ValueFromRatio(clock, speed);
               });

            // UI
            m_modulesPanel = AddPanelWithHeader();
            m_modulesPanel.Header.Add(
                new Label(new Mafi.Localization.LocStrFormatted("Modules"))
                    .FlexGrow(1)
                    .TextAlign(TextAlignment.CenterMiddle)
                );
            m_modulesPanel.Add(m_view = new ControllerView(this, Refresh).AlignSelfCenter());

            HeaderButtons.AddAndReturn(new ButtonIcon(Button.Header, Mafi.Unity.Assets.Unity.UserInterface.General.Connect128_png))
                .OnClick(() => GlobalDependencyResolver.Get<ConnectionInfo>().Open(context.UiRoot))
                .OnMouseEnterLeave(AddPreviewHighlightAll, ClearPreviewHighlight);

            EmbedStatusToTheTop();

            this.Observe(() => Entity)
                .Observe(() => Entity?.Modules)
                .Observe(() => Entity?.Rows)
                .Do((entity, module, rows) => Refresh());

            this.Observe(() => Entity?.State)
                .Do((state) =>
                {
                    Status.As(state ?? Tr.EntityStatus__Working, DisplayState.Positive);
                });
        }

        private void Refresh()
        {
            if (Entity is null)
                m_view.RedrawComponents();
        }

        public CursorManager CursorManager { get; }
        public CursorPickingManager CursorPickingManager { get; }
        //public TerrainCursor TerrainCursor { get; }
        public EntityHighlighter EntityHighlighter { get; }
        public EntityHighlighter EntityHighlighterSelectable { get; }
        public ShortcutsManager ShortcutsManager { get; }
        public EntitySelector EntitySelectionInput { get; set; }
        public ModuleConnector OutputConnection { get; internal set; }
        public CameraController CameraController {  get; }

        public override bool InputUpdate()
        {
            if (EntitySelectionInput != null)
            {
                //var position = TerrainCursor.Tile3f.ToVector3();
                // set line view endpoint

                if (ShortcutsManager.IsPrimaryActionDown)
                {
                    Tile3f source = Entity.Position3f;

                    Option<IRenderedEntity> pickedEntity = CursorPickingManager.PickEntity<IRenderedEntity>(e => EntitySelectionInput.EntityFilter((Entity)e));
                    if (pickedEntity.HasValue
                        && IsWithingDistance(source, pickedEntity.Value, EntitySelectionInput.EntitySearchDistance))
                    {
                        EntitySelectionInput.Entity = pickedEntity.Value;
                        EntitySelectionInput.Refresh();
                        EntitySelectionInput = null;
                        return true;
                    }
                    m_invalidOpSound.Play();
                    return true;
                }
                if (ShortcutsManager.IsSecondaryActionDown)
                {
                    EntitySelectionInput.Refresh();
                    EntitySelectionInput = null;
                    return true;
                }
            }
            if (OutputConnection != null)
            {
                if (ShortcutsManager.IsSecondaryActionDown)
                {
                    OutputConnection = null;
                    return true;
                }
            }
            return base.InputUpdate();
        }

        protected override void SyncUpdate(GameTime gameTime)
        {
            base.SyncUpdate(gameTime);

            if (EntitySelectionInput != null)
            {
                if (!m_highlightSearched)
                {
                    m_highlightSearched = true;
            
                    Tile3f source = Entity.Position3f;
                    Fix32 innerDistance = EntitySelectionInput.EntitySearchDistance;
            
                    Context.EntitiesManager.GetAllEntitiesOfType<Entity>()
                        .Where(e => e is IEntityWithPosition && e is IRenderedEntity)
                        .Where(e => EntitySelectionInput.EntityFilter(e))
                        .Where(e => IsWithingDistance(source, e as IRenderedEntity, innerDistance))
                        .Call(e => EntityHighlighterSelectable.Highlight(e as IRenderedEntity, ColorRgba.Green))
                        .ToList();
                }
            
                Option<IRenderedEntity> pickedEntity = CursorPickingManager.PickEntity<IRenderedEntity>(e => EntitySelectionInput.EntityFilter((Entity)e));
            
                if (pickedEntity.IsNone || m_hoveredEntity != null)
                {
                    EntityHighlighter.RemoveHighlight(m_hoveredEntity);
                }
            
                if (pickedEntity.HasValue)
                {
                    m_hoveredEntity = pickedEntity.Value;
                    Tile3f source = Entity.Position3f;
            
                    if (IsWithingDistance(source, pickedEntity.Value, EntitySelectionInput.EntitySearchDistance))
                    {
                        EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.LightBlue);
                    }
                    else
                    {
                        EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.Red);
                    }
                    return;
                }
            }
            if (EntitySelectionInput == null && m_hoveredEntity != null)
            {
                EntityHighlighter.RemoveHighlight(m_hoveredEntity);
                m_hoveredEntity = null;
            }
            if (EntitySelectionInput == null && m_highlightSearched)
            {
                EntityHighlighterSelectable.ClearAllHighlights();
                m_highlightSearched = false;
            }
        }

        private bool IsWithingDistance(Tile3f source, IRenderedEntity target, Fix32 searchDistance)
        {
            if (target is IStaticEntity entity && entity.OccupiedTiles
                .Select(t => entity.Position3f.AddX(t.RelativeX).AddY(t.RelativeY))
                .FirstOrDefault(t => (source - t).Length <= searchDistance) != Tile3f.Zero)
                return true;

            return false;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            //EntitySelectionInput = null;
        }

        internal void AddPreviewHighlightAll()
        {
            Dictionary<EntityId, IEntity> entities = new Dictionary<EntityId, IEntity>();
            foreach (var module in Entity.Modules)
            {
                GetEntitiesOfModule(entities, module);
            }
            AddPreviewHighlightOfEntities(entities.Values);
            m_showsLinks = true;
        }

        internal void AddPreviewHighlight(Module module)
        {
            Dictionary<EntityId, IEntity> entities = new Dictionary<EntityId, IEntity>();
            GetEntitiesOfModule(entities, module);
            AddPreviewHighlightOfEntities(entities.Values);
        }

        internal void ClearPreviewHighlight()
        {
            EntityHighlighter.ClearAllHighlights();
            ClearAllLines();
            m_showsLinks = false;
        }

        private static void GetEntitiesOfModule(Dictionary<EntityId, IEntity> entities, Module module)
        {
            foreach (var item in module.Prototype.Fields.Where(f => f is EntityField))
            {
                if (module.Field.Entity<IEntity>(item.Id) is IRenderedEntity entity)
                {
                    entities[entity.Id] = entity;
                }
            }
        }

        private void AddPreviewHighlightOfEntities(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                EntityHighlighter.Highlight(entity as IRenderedEntity, ColorRgba.CornflowerBlue);

                if (entity is Transport transport)
                {
                    var line = m_linesFactory.CreateLine(transport.StartPosition.ToCenterVector3(), Entity.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
                    line.SetTextureMode(LineTextureMode.Tile);
                    m_lines.Add(line);
                }
                else
                {
                    entity.HasPosition(out Tile3f position);
                    var line = m_linesFactory.CreateLine(position.ToVector3(), Entity.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
                    line.SetTextureMode(LineTextureMode.Tile);
                    m_lines.Add(line);
                }
            }
        }

        private void ClearAllLines()
        {
            foreach (var line in m_lines)
                UnityEngine.Object.Destroy(line.gameObject);
            m_lines.Clear();
        }
    }
}
