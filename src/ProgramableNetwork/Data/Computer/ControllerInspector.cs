using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Input;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Inspectors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class ControllerInspector : EntityInspector<Controller, ControllerView>, ISelectionInspector<IEntity, EntitySelector, Controller>
    {
        private readonly ControllerView m_windowView;
        private readonly AudioSource m_invalidOpSound;
        private bool m_highlightSearched;
        private IRenderedEntity m_hoveredEntity;
        private readonly LinesFactory m_linesFactory;
        private readonly List<LineMb> m_lines = new List<LineMb>();
        private readonly Material m_movingArrowsLineMaterialShared;

        public ControllerInspector(
            InspectorContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager,
            //TerrainCursor terrainCursor,
            NewInstanceOf<EntityHighlighter> entityHighlighter,
            NewInstanceOf<EntityHighlighter> entityHighlighterSelectable,
            LinesFactory linesFactory,
            AssetsDb assetsDb
            ) : base(context)
        {
            m_windowView = new ControllerView(this);
            m_linesFactory = linesFactory;
            m_movingArrowsLineMaterialShared = assetsDb.GetSharedMaterial("Assets/Core/Materials/MovingArrowsLine.mat");
            CursorManager = cursorManager;
            CursorPickingManager = cursorPickingManager;
            //TerrainCursor = terrainCursor;
            EntityHighlighter = entityHighlighter.Instance;
            EntityHighlighterSelectable = entityHighlighterSelectable.Instance;
            ShortcutsManager = shortcutsManager;
            m_invalidOpSound = Context.Builder.AudioDb.GetSharedAudio(Context.Builder.Audio.InvalidOp);
        }

        public CursorManager CursorManager { get; }
        public CursorPickingManager CursorPickingManager { get; }
        //public TerrainCursor TerrainCursor { get; }
        public EntityHighlighter EntityHighlighter { get; }
        public EntityHighlighter EntityHighlighterSelectable { get; }
        public ShortcutsManager ShortcutsManager { get; }
        public EntitySelector EntitySelectionInput { get; set; }

        public override ControllerView GetView()
        {
            return m_windowView;
        }

        public override bool InputUpdate(IInputScheduler inputScheduler)
        {
            if (EntitySelectionInput != null)
            {
                //var position = TerrainCursor.Tile3f.ToVector3();
                // set line view endpoint

                if (ShortcutsManager.IsPrimaryActionDown)
                {
                    Tile3f source = SelectedEntity.Position3f;

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
            return base.InputUpdate(inputScheduler);
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            if (EntitySelectionInput != null)
            {
                if (!m_highlightSearched)
                {
                    m_highlightSearched = true;
            
                    Tile3f source = SelectedEntity.Position3f;
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
                    Tile3f source = SelectedEntity.Position3f;
            
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
            foreach (var module in SelectedEntity.Modules)
            {
                GetEntitiesOfModule(entities, module);
            }
            AddPreviewHighlightOfEntities(entities.Values);
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
                entity.HasPosition(out Tile3f position);
                var line = m_linesFactory.CreateLine(position.ToVector3(), SelectedEntity.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
                line.SetTextureMode(LineTextureMode.Tile);
                m_lines.Add(line);
            }
        }

        private void ClearAllLines()
        {
            foreach (var line in m_lines)
                line.gameObject.Destroy();
            m_lines.Clear();
        }
    }
}
