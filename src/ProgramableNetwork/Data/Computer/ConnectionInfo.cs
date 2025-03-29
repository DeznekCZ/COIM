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
        private readonly List<IDataUpdater> m_updaters;
        private readonly InspectorContext m_inspectorContext;
        private readonly EntityHighlighter m_entityHighlighter;
        private readonly IGameLoopEvents m_gameLoop;

        public ScrollableStackContainer ScrollableItemsContainer { get; private set; }
        public StackContainer ItemsContainer { get; private set; }

        public ConnectionInfo(VariableManager variableManager, IUnityInputMgr unityInput, InspectorContext inspectorContext, UiBuilder uiBuilder, IGameLoopEvents gameLoop, NewInstanceOf<EntityHighlighter> entityHighlighter)
            : base("connections", FooterStyle.Round, false)
        {
            m_inspectorContext = inspectorContext;
            m_entityHighlighter = entityHighlighter.Instance;
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
            foreach (var item in controllers)
            {
                m_controllers.AppendDivider(2, Style.EntitiesMenu.MenuBg);

                StackContainer container = Builder.NewStackContainer(DateTime.Now.Ticks.ToString())
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetHeight(40)
                    .SetWidth(500)
                    .AppendTo(m_controllers);

                Builder.NewBtnGeneral("controller_" + item.Id.Value)
                    //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                    .OnClick(() => m_inspectorContext.CameraController.PanTo(item.Position2f))
                    .SetSize(40, 40)
                    .SetButtonStyle(Builder.Style.Global.ImageBtn)
                    .SetIcon(item.Prototype.IconPath)
                    .SetOnMouseEnterLeaveActions(
                        () => m_entityHighlighter.HighlightOnly(item, ColorRgba.CornflowerBlue),
                        () => m_entityHighlighter.ClearAllHighlights()
                    )
                    .AppendTo(container);

                GridContainer linkcontainer = Builder.NewGridContainer(DateTime.Now.Ticks.ToString())
                    .SetCellSize(new Vector2(40, 40))
                    .SetWidth(400)
                    .AppendTo(container)
                    .SetDynamicHeightMode(10);

                List<(Module module, List<EntityField> fields)> list = item.Modules
                    .Select(module => (
                        module,
                        module.Prototype.Fields
                            .Where(field => field is EntityField)
                            .Cast<EntityField>()
                            .ToList()
                    ))
                    .ToList();

                container.SetHeight((list.Sum((data) => data.fields.Count) / 10 + 1) * 40);
                foreach (var (module, fields) in list)
                {
                    foreach (EntityField field in fields)
                    {
                        IEntity entity = module.Field.Entity<IEntity>(field.Id);

                        m_updaters.Add(new DataUpdaterChecked<EntityId?, (Module module, EntityField field)>(
                            (c) => c.module.Field.Entity<IEntity>(c.field.Id)?.Id,
                            (c, s) => Refresh(new Lyst<string>()),
                            (a, b) => a != b,
                            (module, field),
                            entity?.Id
                        ));

                        Builder.NewBtnGeneral("controller_" + item.Id.Value + "_module_" + module.Id + "_field_" + field.Id)
                            //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                            .OnClick(() => m_inspectorContext.CameraController.PanTo(entity.HasPosition(out Tile2f position)
                                                ? position : item.Position2f))
                            .SetSize(40, 40)
                            .SetButtonStyle(Builder.Style.Global.ImageBtn)
                            .SetIcon(entity?.Prototype is IProtoWithIcon withIcon
                                        ? withIcon.IconPath : Assets.Unity.UserInterface.General.Empty128_png)
                            .SetOnMouseEnterLeaveActions(
                                () => m_entityHighlighter.HighlightOnly(entity as IRenderedEntity, ColorRgba.CornflowerBlue),
                                () => m_entityHighlighter.ClearAllHighlights()
                            )
                            .AppendTo(linkcontainer);
                    }
                }
            }

            Builder.NewStackContainer("filler")
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .AppendTo(m_controllers);
        }
    }
}