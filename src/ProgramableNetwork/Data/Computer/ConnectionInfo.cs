using Mafi;
using Mafi.Core;
using Mafi.Core.Entities.Static;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mafi.Unity.Entities;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    [GlobalDependency(RegistrationMode.AsSelf)]
    public class ConnectionInfo : WindowView
    {
        private StackContainer m_controllers;
        private Option<ScrollableStackContainer> m_scrollableStackContainer;
        private Option<Controller> m_selected;
        private readonly Material m_movingArrowsLineMaterialShared;
        private readonly List<IDataUpdater> m_updaters;
        private readonly InspectorContext m_inspectorContext;
        private readonly EntityHighlighter m_entityHighlighter;
        private readonly LinesFactory m_linesFactory;
        private readonly List<LineMb> m_lines = new List<LineMb>();
        private readonly AudioSource m_invalidOpSound;

        public ScrollableStackContainer ScrollableItemsContainer { get; private set; }
        public StackContainer ItemsContainer { get; private set; }

        public ConnectionInfo(IUnityInputMgr unityInput, InspectorContext inspectorContext,
            UiBuilder uiBuilder, NewInstanceOf<EntityHighlighter> entityHighlighter,
            LinesFactory linesFactory, AssetsDb assetsDb)
            : base("connections", FooterStyle.Round, false)
        {
            m_inspectorContext = inspectorContext;
            m_movingArrowsLineMaterialShared = assetsDb.GetSharedMaterial("Assets/Core/Materials/MovingArrowsLine.mat");
            m_invalidOpSound = uiBuilder.AudioDb.GetSharedAudio(uiBuilder.Audio.InvalidOp);
            m_entityHighlighter = entityHighlighter.Instance;
            m_linesFactory = linesFactory;
            m_updaters = new List<IDataUpdater>();
            unityInput.RegisterGlobalShortcut(
                m => KeyBindings.FromPrimaryKeys(KbCategory.General, ShortcutMode.Game, KeyCode.LeftControl, KeyCode.W),
                () =>
                {
                    BuildAndShow(uiBuilder);
                });
            SetOnCloseButtonClickAction(Hide);
            OnShowStart += VariableWindow_OnShowStart;
            OnHide += VariableWindow_OnHide;
        }

        private void VariableWindow_OnHide()
        {
            m_inspectorContext.EntitiesManager.StaticEntityAdded.RemoveNonSaveable(this, EntitiesChanged);
            m_inspectorContext.EntitiesManager.StaticEntityRemoved.RemoveNonSaveable(this, EntitiesChanged);
            ClearAllLines();
        }

        private void ClearAllLines()
        {
            foreach (var line in m_lines)
                line.gameObject.Destroy();
            m_lines.Clear();
        }

        private void VariableWindow_OnShowStart()
        {
            m_inspectorContext.EntitiesManager.StaticEntityAdded.AddNonSaveable(this, EntitiesChanged);
            m_inspectorContext.EntitiesManager.StaticEntityRemoved.AddNonSaveable(this, EntitiesChanged);
            Refresh();
        }

        private void EntitiesChanged(IStaticEntity entity)
        {
            Refresh();
        }

        protected override void BuildWindowContent()
        {
            SetTitle("Controller menu");
            PositionSelfToCenter();
            MakeMovable();

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            ItemsContainer = Builder.NewStackContainer("StackContainer")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetWidth(500)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .PutTo(GetContentPanel());

            // TODO search bar
            m_controllers = Builder.NewStackContainer("variables")
                .SetStackingDirection(StackContainer.Direction.TopToBottom);

            ScrollableItemsContainer = new ScrollableStackContainer(Builder, 500, m_controllers)
                .SetWidth(500)
                .SetHeight(500)
                .AppendTo(ItemsContainer);
            ScrollableItemsContainer.SizeChanged += delegate
            {
                SetContentSize(500, 500);
            };

            Refresh();

            updaterBuilder
                .Observe(() => DateTime.Now.Ticks)
                .Do((l) => m_updaters.ForEach(u => u.Update()));

            AddUpdater(updaterBuilder.Build());

            ItemsContainer.SizeChanged += delegate
            {
                SetContentSize(500, 500);
            };
            SetContentSize(500, 500);
        }

        private void Refresh()
        {
            m_updaters.Clear();
            m_controllers.ClearAndDestroyAll();

            var height = 0;
            m_controllers.AppendDivider(2, Style.EntitiesMenu.MenuBg);
            height += 2;

            var controllers = m_inspectorContext.EntitiesManager.GetAllEntitiesOfType<Controller>();
            foreach (var controller in controllers)
            {
                StackContainer container = Builder.NewStackContainer(DateTime.Now.Ticks.ToString())
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetWidth(500);

                var controllerButton = Builder.NewBtnGeneral("controller_" + controller.Id.Value)
                    //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                    .OnClick(() => m_inspectorContext.CameraController.PanTo(controller.Position2f))
                    .OnDoubleClick(() =>
                    {
                        m_selected = controller;
                        InspectorContext inspectorContext = GlobalDependencyResolver.Get<InspectorContext>();
                        if (inspectorContext.MainController.TryActivateFor(inspectorContext.InputMgr, controller))
                            inspectorContext.InputMgr.ActivateNewController(inspectorContext.MainController);
                        else
                            m_invalidOpSound.Play();
                    })
                    .SetSize(40, 40)
                    .SetButtonStyle(Builder.Style.Global.ImageBtn)
                    .SetIcon(controller.Prototype.IconPath)
                    .AppendTo(container);

                GridContainer linkcontainer = Builder.NewGridContainer(DateTime.Now.Ticks.ToString())
                    .SetCellSize(new Vector2(40, 40))
                    .SetWidth(400)
                    .AppendTo(container)
                    .SetDynamicHeightMode(10);

                List<(Module module, List<EntityField> fields)> list = controller.Modules
                    .Select(module => (
                        module,
                        module.Prototype.Fields
                            .Where(field => field is EntityField)
                            .Cast<EntityField>()
                            .ToList()
                    ))
                    .ToList();

                Dictionary<EntityId, IEntity> allEntities = new Dictionary<EntityId, IEntity>();
                foreach (var (module, fields) in list)
                {
                    foreach (EntityField field in fields)
                    {
                        IEntity entity = module.Field.Entity<IEntity>(field.Id);
                        if (entity?.HasPosition(out Tile3f _) ?? false)
                            allEntities[entity.Id] = entity;

                        m_updaters.Add(new DataUpdaterChecked<EntityId?, (Module module, EntityField field)>(
                            (c) => c.module.Field.Entity<IEntity>(c.field.Id)?.Id,
                            (c, s) => Refresh(),
                            (a, b) => a == b,
                            (module, field),
                            entity?.Id
                        ));
                    }
                }

                container.SetHeight(((allEntities.Count - 1) / 10 + 1) * 40);
                container.AppendTo(m_controllers);
                height += ((allEntities.Count - 1) / 10 + 1) * 40;

                foreach (var entity in allEntities.Values)
                {
                    Builder.NewBtnGeneral("controller_" + controller.Id.Value + "_entity_" + entity.Id.Value)
                        //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                        .OnClick(() => m_inspectorContext.CameraController.PanTo(entity.HasPosition(out Tile2f position)
                                            ? position : controller.Position2f))
                        .OnDoubleClick(() =>
                        {
                            InspectorContext inspectorContext = GlobalDependencyResolver.Get<InspectorContext>();
                            if (inspectorContext.MainController.TryActivateFor(inspectorContext.InputMgr, entity as IRenderedEntity))
                                inspectorContext.InputMgr.ActivateNewController(inspectorContext.MainController);
                            else
                                m_invalidOpSound.Play();
                        })
                        .SetSize(40, 40)
                        .SetButtonStyle(Builder.Style.Global.ImageBtn)
                        .SetIcon(entity?.Prototype is IProtoWithIcon withIcon
                                    ? withIcon.IconPath : Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png)
                        .SetOnMouseEnterLeaveActions(
                            () =>
                            {
                                ClearAllLines();
                                m_entityHighlighter.ClearAllHighlights();
                                m_entityHighlighter.Highlight(controller, ColorRgba.Yellow);
                                m_entityHighlighter.Highlight(entity as IRenderedEntity, ColorRgba.CornflowerBlue);
                                entity.HasPosition(out Tile3f position);
                                var line = m_linesFactory.CreateLine(position.ToVector3(), controller.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
                                line.SetTextureMode(LineTextureMode.Tile);
                                m_lines.Add(line);
                            },
                            () =>
                            {
                                ClearAllLines();
                                m_entityHighlighter.ClearAllHighlights();
                            }
                        )
                        .AppendTo(linkcontainer);
                }

                controllerButton
                    .SetOnMouseEnterLeaveActions(
                        () =>
                        {
                            m_entityHighlighter.ClearAllHighlights();
                            m_entityHighlighter.Highlight(controller, ColorRgba.Yellow);
                            foreach (var entity in allEntities.Values)
                            {
                                entity.HasPosition(out Tile3f position);
                                var line = m_linesFactory.CreateLine(position.ToVector3(), controller.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
                                line.SetTextureMode(LineTextureMode.Tile);
                                m_lines.Add(line);
                                m_entityHighlighter.Highlight(entity as IRenderedEntity, ColorRgba.CornflowerBlue);
                            }
                        },
                        () =>
                        {
                            ClearAllLines();
                            m_entityHighlighter.ClearAllHighlights();
                        }
                    );

                m_controllers.AppendDivider(2, Style.EntitiesMenu.MenuBg);
                height += 2;
            }

            m_controllers.SetHeight(height);
        }
    }
}