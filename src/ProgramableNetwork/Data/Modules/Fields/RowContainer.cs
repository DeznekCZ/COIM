using Mafi;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork
{
    public struct RowContainer
    {
        private Action<UiComponent> m_add;
        private Action<Px> m_gap;
        private UiComponent m_component;

        public void Add(UiComponent component)
        {
            m_add.Invoke(component);
        }

        public T AddAndReturn<T>(T component)
            where T : UiComponent
        {
            m_add.Invoke(component);
            return component;
        }

        public RowContainer Gap(Px mainAxis)
        {
            m_gap.Invoke(mainAxis);
            return this;
        }

        public static implicit operator UiComponent(RowContainer container) => container.m_component;
        public static implicit operator RowContainer(Row container) => new RowContainer
        {
            m_component = container,
            m_add = (element) => container.Add(element),
            m_gap = (gap) => container.Gap(gap)
        };
        public static implicit operator RowContainer(PanelRow container) => new RowContainer
        {
            m_component = container,
            m_add = (element) => container.BodyAdd(element),
            m_gap= (gap) => container.BodyGap(gap)
        };
    }
}