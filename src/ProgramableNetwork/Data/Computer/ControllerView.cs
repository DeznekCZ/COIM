using Mafi;
using Mafi.Core;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using System;
using System.Collections.Generic;
using Mafi.Unity.UiFramework;
using Mafi.Core.Syncers;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;

namespace ProgramableNetwork
{
    public partial class ControllerView : StaticEntityInspectorBase<Controller>
    {
        private readonly ControllerInspector m_controller;
        private readonly Dictionary<IUiElement, Action> m_displayChanges;
        private ISyncer<Controller> selectionchanged;
        private bool m_repaintInstructions;
        private bool m_debugging;
        public ControllerView(ControllerInspector controller)
            : base(controller)
        {
            m_controller = controller;
            m_displayChanges = new Dictionary<IUiElement, Action>();
        }

        protected override Controller Entity => m_controller.SelectedEntity;

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            //MakeScrollableWithHeightLimit();
            base.AddCustomItems(itemContainer);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            var instruction = AddStatusInfoPanel();
            updaterBuilder.Observe(() => m_controller.SelectedEntity?.Modules?.Count ?? 0)
                .Do(count => instruction.SetStatus(NewTr.ModulesCount.Format(count.ToString()).Value, count == 0 ? StatusPanel.State.Warning : StatusPanel.State.Ok));

            var status = AddStatusInfoPanel();
            updaterBuilder.Observe(() =>
                    (m_controller.SelectedEntity?.ElectricityConsumer.ValueOrNull?.NotEnoughPower ?? false) ||
                    !(m_controller.SelectedEntity?.ErrorMessage?.IsNullOrEmpty() ?? true) ||
                    (m_controller.SelectedEntity?.IsPaused ?? false)
                )
                .Do(noElectricityOrError => {
                    if (!noElectricityOrError)
                        status.SetStatusWorking();
                    else if (!m_controller.SelectedEntity.ErrorMessage.IsEmpty())
                        status.SetStatus(m_controller.SelectedEntity.ErrorMessage, StatusPanel.State.Critical);
                    else if (m_controller.SelectedEntity.IsPaused)
                        status.SetStatus(Tr.EntityStatus__Working, StatusPanel.State.Critical);
                    else
                        status.SetStatusWorking();
                });

            AddGeneralPriorityPanel(m_controller.Context, () => m_controller.SelectedEntity);

            itemContainer.AppendDivider(5, Style.EntitiesMenu.MenuBg);

            var nextRowStack = Builder.NewStackContainer("")
                .SetHeight(20)
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .AppendTo(itemContainer);

            var speed = Builder.NewBtnGeneral("Speed")
                .SettingFieldNameStyle(Builder)
                .SetSize(100, 20)
                .SetText($"{8.0 / (1 + (Entity?.Speed ?? 0))} t/s")
                .ToolTip(m_controller, () =>
                {
                    var value = (1f + (Entity?.Speed ?? 0)) / 8f * 1000f;
                    if (value >= 1000)
                        return $"Time between actions: {value/1000:F0} s";
                    else
                        return $"Time between actions: {value:F0} ms";
                })
                .AppendTo(nextRowStack);

            var speedDown = Builder.NewBtnGeneral("SpeedDown")
                .SetText("-")
                .SetSize(20, 20)
                .OnClick(() =>
                {
                    if (Entity?.Speed == 511)
                        return; // or play sound

                    Entity.Speed = (Entity.Speed + 1) * 2 - 1;
                })
                .AppendTo(nextRowStack);

            var speedUp = Builder.NewBtnGeneral("SpeedUp")
                .SetText("+")
                .SetSize(20, 20)
                .OnClick(() =>
                {
                    if (Entity?.Speed == 0)
                        return; // or play sound

                    Entity.Speed = (Entity.Speed + 1) / 2 - 1;
                })
                .AppendTo(nextRowStack);

            updaterBuilder.Observe(() => Entity?.Speed)
                .Do(speedValue =>
                {
                    speed.SetText($"{8.0 / (1 + Entity.Speed):0.000} t/s");
                    speedUp.SetEnabled(Entity.Speed > 0);
                    speedDown.SetEnabled(Entity.Speed < 511);
                });

            nextRowStack.AppendDivider(5, Style.EntitiesMenu.MenuBg);

            Builder.NewSwitchBtn()
                .SetText("Log output")
                .SetIsOn(m_debugging)
                .SetOnToggleAction(b => m_debugging = b)
                .SetSize(100, 20)
                .AppendTo(nextRowStack);

            itemContainer.AppendDivider(5, Style.EntitiesMenu.MenuBg);

            AddModuleImplementation(itemContainer, updaterBuilder, (width, height) =>
            {
                SetContentSize(width, height + 80);
            });

            selectionchanged = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity);

            AddUpdater(updaterBuilder.Build(SyncFrequency.Critical));
            m_repaintInstructions = true;
        }

        public override void SyncUpdate(GameTime gameTime)
        {
            base.SyncUpdate(gameTime);
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            if (m_controller.SelectedEntity != null && Builder != null && (m_repaintInstructions || selectionchanged.HasChanged))
            {
                selectionchanged.GetValueAndReset();

                m_repaintInstructions = false;
            }

            if (m_controller.SelectedEntity != null)
            {
                foreach (var item in m_updaters)
                    item.Update();

                foreach (var d in new List<Action>(m_displayChanges.Values))
                    d.Invoke();
            }
        }
    }
}
