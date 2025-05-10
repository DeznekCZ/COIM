using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Unity.InputControl.Inspectors;
using System;
using Newtonsoft.Json;
using Mafi.Core.Entities.Static;
using System.Linq;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Core.Syncers;

namespace ProgramableNetwork
{
    public class EntityField : IField
    {
        private string id;
        private string name;
        private string shortDesc;
        private Func<Module, IEntity, bool> entitySelector;
        private Fix32 distance;
        private Fix64 sqrDistance;

        public EntityField(string id, string name, string shortDesc, Func<Module, IEntity, bool> entitySelector, Fix32 distance)
        {
            this.id = id;
            this.name = name;
            this.shortDesc = shortDesc;
            this.entitySelector = entitySelector;
            this.distance = distance;
            this.sqrDistance = distance.ToFix64() * distance.ToFix64();
        }

        public string Id => id;

        public string Name => name;
        public string ShortDesc => shortDesc;

        public int Size => 40;

        public void Validate(Module module)
        {
            EntityInfo entityData = null;
            // FOR ONLY NEWLY CONSTRUCTED
            module.NumberData.TryRemove("field__" + Id, out _);
            if (module.StringData.TryGetValue("field__" + Id, out var value))
            {
                Log.Info("Searching for entity in module by config: " + module.Id + " with key: " + Id);
                entityData = JsonConvert.DeserializeObject<EntityInfo>(value);

                foreach (IEntity entity in module.Controller.Context.EntitiesManager.Entities)
                {
                    if (entitySelector.Invoke(module, entity))
                    {
                        entity.HasPosition(out Tile3f entityBlockTile);
                        entity.HasPosition(out Tile2f entityTile);

                        if (entityData.Z == int.MinValue)
                        {
                            if (module.Controller.Position2f - entityTile == entityData.Relative2)
                            {
                                Log.Info("Found by definition");
                                module.Field.Entity(Id, entity); // set by position
                                return; // BUT position changed
                            }
                        }
                        else
                        {
                            if (module.Controller.Position3f - entityBlockTile == entityData.Relative)
                            {
                                Log.Info("Found by definition");
                                module.Field.Entity(Id, entity); // set by position
                                return; // BUT position changed
                            }
                        }
                    }
                }
                Log.Info("Not found");
            }
            else
            {
                Log.Info("Entity is not set: " + Id + " with key: " + Id);
            }
        }

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog)
        {
            Row row = new Row();
            row.Height(40);
            fieldContainer.Add(row);

            Label label = new Label();
            label.Value(new Mafi.Localization.LocStrFormatted(Name));
            label.Tooltip(new Mafi.Localization.LocStrFormatted(ShortDesc));
            label.Size(width: 180, height: 40);
            row.Add(label);

            Picker picker = new Picker(module, id, entitySelector, distance, updateDialog, parentWindow, inspector);
            row.Add(picker);
        }

        public void InitData(Module module)
        {
            // nothing to do
        }

        public class Picker : Row
        {
            private readonly Func<Module, IEntity, bool> m_filter;
            private readonly Action m_refresh;
            private readonly Window m_window;
            private readonly ControllerInspector m_inspector;
            private readonly UiContext m_UiContext;
            private readonly Module m_module;
            private readonly string m_dataName;
            private readonly Fix32 m_distance;
            private readonly ButtonIcon m_selectionButton;
            private readonly ButtonIcon m_btnPreview;

            public Picker(Module module, string dataName, Func<Module, IEntity, bool> filter, Fix32 distance, Action refresh, Window parentWindow, ControllerInspector inspector)
                : base()
            {
                m_filter = filter;
                m_refresh = refresh;
                m_window = parentWindow;
                m_inspector = inspector;
                m_UiContext = inspector.Context;
                m_module = module;
                m_dataName = dataName;
                m_distance = distance;
                parentWindow.OnCloseStart += ParentWindow_OnCloseStart;

                this.Size(80, 40);

                m_btnPreview = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnPreview.Size(40, 40);
                m_btnPreview.OnClick(() =>
                {
                    Entity entity = m_module.Field.Entity<Entity>(m_dataName);
                    if (entity.HasPosition(out Tile2f position))
                        m_UiContext.CameraController.PanTo(position);
                });
                Add(m_btnPreview);

                m_selectionButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Edit_svg);
                m_selectionButton.Size(40, 40);
                m_selectionButton.OnClick(PickEntity);
                Add(m_selectionButton);

                m_btnPreview.OnMouseEnterLeave(
                    () =>
                    {
                        Entity entity = m_module.Field.Entity<Entity>(m_dataName);
                        if (entity is null) return;
                        m_UiContext.Highlighter.Highlight(
                            (IRenderedEntity)entity, ColorRgba.LightBlue);
                    },
                    () =>
                    {
                        Entity entity = m_module.Field.Entity<Entity>(m_dataName);
                        if (entity is null) return;
                        m_UiContext.Highlighter.RemoveHighlight(
                            (IRenderedEntity)entity);
                    });
                m_selectionButton.OnMouseEnterLeave(
                    () =>
                    {
                        Entity entity = m_module.Field.Entity<Entity>(m_dataName);
                        if (entity is null) return;
                        m_UiContext.Highlighter.Highlight(
                            (IRenderedEntity)entity, ColorRgba.LightBlue);
                    },
                    () =>
                    {
                        Entity entity = m_module.Field.Entity<Entity>(m_dataName);
                        if (entity is null) return;
                        m_UiContext.Highlighter.RemoveHighlight(
                            (IRenderedEntity)entity);
                    });

                m_inspector.EntitySelectionInput = null;

                this.Observe(() => m_module.Field[m_dataName])
                    .Do((item) => Refresh());
            }

            private void ParentWindow_OnCloseStart(Window obj)
            {
                m_inspector.EntitySelectionInput = null;
            }

            protected override void OnDetached()
            {
                m_window.OnCloseStart -= ParentWindow_OnCloseStart;
                base.OnDetached();
            }

            private void PickEntity()
            {
                m_selectionButton.Class(Cls.selected);
                m_inspector.EntitySelectionInput = new EntitySelector(m_module, m_distance, m_refresh, m_filter,
                    (entity) =>
                    {
                        m_selectionButton.ClassRemove(Cls.selected);
                        m_module.Field.Entity(m_dataName, entity);
                        m_refresh();
                    });
            }

            private void Refresh()
            {
                Entity entity = m_module.Field.Entity<Entity>(m_dataName);
                if (entity is null)
                    m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                else
                    m_btnPreview.Icon.Value(entity.GetIcon());
            }
        }
    }
}