using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using Newtonsoft.Json;
using Mafi.Core.Entities.Static;
using System.Linq;

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

        public void Init(ControllerInspector inspector, ItemDetailWindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            fieldContainer.SetStackingDirection(StackContainer.Direction.LeftToRight);
            fieldContainer.SetHeight(40);

            var txt = uiBuilder
                .NewBtnGeneral("name")
                .SettingFieldNameStyle(uiBuilder)
                .SetParent(fieldContainer, true)
                .SetWidth(180)
                .SetHeight(40)
#pragma warning disable CS0618 // Typ nebo člen je zastaralý.
                .SetText(Name)
#pragma warning restore CS0618 // Typ nebo člen je zastaralý.
                .ToolTip(inspector, shortDesc, attached: true)
                .AppendTo(fieldContainer);

            new Picker(uiBuilder, module, id, entitySelector, distance, updateDialog, parentWindow, inspector)
                .AppendTo(fieldContainer);
        }

        public void InitData(Module module)
        {
            // nothing to do
        }

        public class Picker : StackContainer
        {
            private readonly UiBuilder m_builder;
            private readonly Func<Module, IEntity, bool> m_filter;
            private readonly Action m_refresh;
            private readonly WindowView m_window;
            private readonly ControllerInspector m_inspector;
            private readonly InspectorContext m_inspectorContext;
            private readonly Module m_module;
            private readonly string m_dataName;
            private readonly Fix32 m_distance;
            private readonly StackContainer m_btnPreviewHolder;
            private readonly Btn m_selectionButton;
            private Btn m_btnPreview;

            public Picker(UiBuilder builder, Module module, string dataName, Func<Module, IEntity, bool> filter, Fix32 distance, Action refresh, WindowView parentWindow, ControllerInspector inspector)
                : base(builder, "entity_" + DateTime.Now.Ticks)
            {
                m_builder = builder;
                m_filter = filter;
                m_refresh = refresh;
                m_window = parentWindow;
                m_inspector = inspector;
                m_inspectorContext = inspector.Context;
                m_module = module;
                m_dataName = dataName;
                m_distance = distance;

                m_btnPreviewHolder = m_builder
                    .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                    .SetStackingDirection(Direction.LeftToRight)
                    .SetSizeMode(SizeMode.Dynamic)
                    .SetSize(80, 40)
                    .AppendTo(this);

                m_btnPreview = m_builder
                    .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                    .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                    .SetSize(40, 40)
                    .SetIcon(m_builder.Style.Icons.Empty)
                    .AppendTo(m_btnPreviewHolder);

                m_selectionButton = m_builder
                    .NewBtnGeneral("item_vehicle_" + DateTime.Now.Ticks)
                    .SetText("Pick")
                    .SetSize(40, 40)
                    .OnClick(PickEntity)
                    .AppendTo(m_btnPreviewHolder);

                SetSizeMode(SizeMode.Dynamic);
                this.SetHeight(40);

                m_window.OnHide += onHide;

                m_selectionButton.SetButtonStyle(m_builder.Style.Global.GeneralBtn);
                m_inspector.EntitySelectionInput = null;

                Entity entity = m_module.Field.Entity<Entity>(m_dataName);
                if (entity != null)
                {
                    m_btnPreview.SetIcon(entity.GetIcon());
                    m_btnPreview.OnClick(
                        () =>
                        {
                            if (entity.HasPosition(out Tile2f position))
                                m_inspectorContext.CameraController.PanTo(position);
                        });
                    m_btnPreview.SetOnMouseEnterLeaveActions(
                        () =>
                        {
                            m_inspectorContext.Highlighter.Highlight(
                                (IRenderedEntity)entity, ColorRgba.LightBlue);
                        },
                        () =>
                        {
                            m_inspectorContext.Highlighter.RemoveHighlight(
                                (IRenderedEntity)entity);
                        });
                    m_selectionButton.SetOnMouseEnterLeaveActions(
                        () =>
                        {
                            m_inspectorContext.Highlighter.Highlight(
                                (IRenderedEntity)entity, ColorRgba.LightBlue);
                        },
                        () =>
                        {
                            m_inspectorContext.Highlighter.RemoveHighlight(
                                (IRenderedEntity)entity);
                        });
                }
                else
                {
                    m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                    m_btnPreview.OnClick(() => { });
                    m_btnPreview.SetOnMouseEnterLeaveActions(() => { }, () => { });
                    m_selectionButton.SetOnMouseEnterLeaveActions(() => { }, () => { });
                }
            }

            private void onHide()
            {
                m_window.OnHide -= onHide;
                m_inspector.EntitySelectionInput = null;
            }

            private void PickEntity()
            {
                m_selectionButton.SetButtonStyle(m_builder.Style.Global.GeneralBtnActive);
                m_inspector.EntitySelectionInput = new EntitySelector(m_module, m_distance, m_refresh, m_filter,
                    (entity) =>
                    {
                        m_module.Field.Entity(m_dataName, entity);
                    });
            }
        }
    }
}