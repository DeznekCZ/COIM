using Mafi.Core.Entities.Commands;
using Mafi.Core.Entities.Priorities;
using Mafi.Core.Input;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;
using System;

namespace MultiplayerContracts
{
    internal class ExportPriorityPanel : BasePriorityPanel
    {
        private readonly IInputScheduler m_inputScheduler;

        private readonly Func<IEntityWithCustomPriority> m_provider;

        private readonly string m_priorityId;

        public ExportPriorityPanel(IUiElement parent, IInputScheduler inputScheduler, UiBuilder builder, Func<IEntityWithCustomPriority> provider, string priorityId)
            : base(parent, builder, 14, Tr.ExportPriority.AsFormatted.Value)
        {
            m_inputScheduler = inputScheduler;
            m_provider = provider;
            m_priorityId = priorityId;
            SetCustomIcon("Assets/Unity/UserInterface/General/Export128.png");
            SetColor(builder.Style.Global.DangerClr);
            SetTooltip(Tr.ExportPriority__ShipyardCargo);
        }

        protected override int GetCurrentPriority()
        {
            return m_provider().GetCustomPriority(m_priorityId);
        }

        protected override bool IsPrioritySupported()
        {
            return m_provider().IsCustomPriorityVisible(m_priorityId);
        }

        protected override void OnValueChanged(int index)
        {
            m_inputScheduler.ScheduleInputCmd(new SetCustomPriorityCmd(m_provider().Id, m_priorityId, index));
        }
    }
}