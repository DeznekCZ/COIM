using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities.Static;
using Mafi.Core.GameLoop;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity.UserInterface;
using Mafi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mafi.Unity.Entities;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork.Data.Variables
{
    [GlobalDependency(RegistrationMode.AsSelf)]
    public class ConnectionInfo : WindowView
    {
        private StackContainer m_controllers;
        private Option<ScrollableStackContainer> m_scrollableStackContainer;
        private readonly Material m_movingArrowsLineMaterialShared;
        private readonly List<IDataUpdater> m_updaters;
        private readonly InspectorContext m_inspectorContext;
        private readonly EntityHighlighter m_entityHighlighter;
        private readonly LinesFactory m_linesFactory;
        private readonly List<LineMb> m_lines = new List<LineMb>();
        private readonly IGameLoopEvents m_gameLoop;

        public ScrollableStackContainer ScrollableItemsContainer { get; private set; }
        public StackContainer ItemsContainer { get; private set; }

        public ConnectionInfo(VariableManager variableManager, IUnityInputMgr unityInput, InspectorContext inspectorContext,
            UiBuilder uiBuilder, IGameLoopEvents gameLoop, NewInstanceOf<EntityHighlighter> entityHighlighter,
            LinesFactory linesFactory, AssetsDb assetsDb)
            : base("connections", FooterStyle.Round, false)
        {
            m_inspectorContext = inspectorContext;
            m_movingArrowsLineMaterialShared = assetsDb.GetSharedMaterial("Assets/Core/Materials/MovingArrowsLine.mat");
            m_entityHighlighter = entityHighlighter.Instance;
            m_linesFactory = linesFactory;
            m_gameLoop = gameLoop;
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
        }

        private void EntitiesChanged(IStaticEntity entity)
        {
            Refresh(new Lyst<string>());
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
                .SetSizeMode(StackContainer.SizeMode.Dynamic);

            ScrollableItemsContainer = new ScrollableStackContainer(Builder, 300, ItemsContainer)
                .SetWidth(500)
                .SetHeight(500)
                .PutTo(GetContentPanel());

            // TODO search bar
            m_controllers = Builder.NewStackContainer("variables")
                .SetParent(ItemsContainer)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .AppendTo(ItemsContainer);
            m_controllers.SizeChanged += (element) =>
            {
                ItemsContainer.SetSize(element.GetSize());
            };

            Refresh(new Lyst<string>());

            updaterBuilder
                .Observe(() => DateTime.Now.Ticks)
                .Do((l) => m_updaters.ForEach(u => u.Update()));

            AddUpdater(updaterBuilder.Build());

            //ItemsContainer.SizeChanged += delegate
            //{
            //    SetContentSize(500, Math.Max(ItemsContainer.GetDynamicHeight(), 300));
            //};
            SetContentSize(500, 600);
        }

        private void Refresh(Lyst<string> lyst)
        {
            m_updaters.Clear();
            m_controllers.ClearAndDestroyAll();

            m_controllers.SetHeight(
                m_inspectorContext.EntitiesManager
                    .GetAllEntitiesOfType<Controller>()
                    .Select(controller => (controller.Modules
                        .SelectMany(module => module.Prototype.Fields)
                        .Where(field => field is EntityField)
                        .Count() / 10 + 1) * 40 + 2
                    ).Sum());

            var controllers = m_inspectorContext.EntitiesManager.GetAllEntitiesOfType<Controller>();
            foreach (var controller in controllers)
            {
                m_controllers.AppendDivider(2, Style.EntitiesMenu.MenuBg);

                StackContainer container = Builder.NewStackContainer(DateTime.Now.Ticks.ToString())
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetWidth(500);

                var controllerButton = Builder.NewBtnGeneral("controller_" + controller.Id.Value)
                    //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                    .OnClick(() => m_inspectorContext.CameraController.PanTo(controller.Position2f))
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

                container
                    .SetHeight((list.Sum((data) => data.fields.Count) / 10 + 1) * 40)
                    .AppendTo(m_controllers);
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
                            (c, s) => Refresh(new Lyst<string>()),
                            (a, b) => a != b,
                            (module, field),
                            entity?.Id
                        ));

                        Builder.NewBtnGeneral("controller_" + controller.Id.Value + "_module_" + module.Id + "_field_" + field.Id)
                            //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                            .OnClick(() => m_inspectorContext.CameraController.PanTo(entity.HasPosition(out Tile2f position)
                                                ? position : controller.Position2f))
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
            }

            Builder.NewStackContainer("filler")
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .AppendTo(m_controllers);
        }
    }
}