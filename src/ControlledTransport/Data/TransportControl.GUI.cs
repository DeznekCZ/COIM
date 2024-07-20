using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Ports;
using Mafi.Core.Ports.Io;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mafi.Unity.UiFramework;
using Mafi.Serialization;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Factory.Transports;

namespace ControlledTransport
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class TransportControlInspector : EntityInspector<TransportControl, TransportControlView>
    {
        private readonly TransportControlView m_windowView;

        public TransportControlInspector(InspectorContext context) : base(context)
        {
            m_windowView = new TransportControlView(this);
        }

        protected override TransportControlView GetView()
        {
            return m_windowView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            m_windowView.SelectionChanged();
        }
    }

    public class TransportControlView : StaticEntityInspectorBase<TransportControl>
    {
        private TransportControlInspector m_controller;
        private TxtField maxField;
        private TxtField minField;

        public TransportControlView(TransportControlInspector controller)
            : base(controller)
        {
            m_controller = controller;
        }

        protected override TransportControl Entity => m_controller.SelectedEntity;

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            base.AddCustomItems(itemContainer);

            maxField = new TxtField(Builder, "Maximum");
            minField = new TxtField(Builder, "Minimum");

            maxField.SetCharLimit(5);
            maxField.SetOnValueChangedAction(() =>
            {
                if (int.TryParse(maxField.GetText(), out int v) && v > 0 && v <= 64000 && m_controller.SelectedEntity.MinimumTroughput < v)
                {
                    if (v <= m_controller.SelectedEntity.MinimumTroughput)
                        maxField.SetText((m_controller.SelectedEntity.MinimumTroughput + 1).ToString());
                    else if (v > 64000)
                        maxField.SetText("64000");
                    else if (v <= 0)
                        maxField.SetText("1");
                    else
                        m_controller.SelectedEntity.MaximumTroughput = v;
                }
                else
                    maxField.SetText(Regex.Replace(maxField.GetText(), @"[^0-9]+", "", RegexOptions.Compiled));
            });
            minField.SetCharLimit(5);
            minField.SetOnValueChangedAction(() =>
            {
                if (int.TryParse(minField.GetText(), out int v) && v >= 0 && v < 64000 && v < m_controller.SelectedEntity.MaximumTroughput)
                {
                    if (v >= m_controller.SelectedEntity.MaximumTroughput)
                        minField.SetText((m_controller.SelectedEntity.MaximumTroughput - 1).ToString());
                    else if (v >= 64000)
                        minField.SetText("63999");
                    else if (v < 0)
                        minField.SetText("0");
                    else
                        m_controller.SelectedEntity.MinimumTroughput = v;
                }
                else
                    minField.SetText(Regex.Replace(minField.GetText(), @"[^0-9]+", "", RegexOptions.Compiled));
            });

            itemContainer.Add(itemContainer.ItemsCount, minField, size: (Vector2?)null, position: (ContainerPosition?)null, offset: Offset.Zero);
            itemContainer.Add(itemContainer.ItemsCount, maxField, size: (Vector2?)null, position: (ContainerPosition?)null, offset: Offset.Zero);
        }

        public void SelectionChanged()
        {
            maxField.SetText(m_controller.SelectedEntity.MaximumTroughput.ToString());
            minField.SetText(m_controller.SelectedEntity.MinimumTroughput.ToString());
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            if (m_controller.SelectedEntity == null)
                return;


        }
    }
}
