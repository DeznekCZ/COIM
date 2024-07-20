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
using Mafi.Core.Syncers;
using Mafi.Core.Products;
using Mafi.Unity.UserInterface.Style;
using Mafi.Localization;

namespace ControlledTransport
{
    public partial class NewIds
    {
        public partial class Texts
        {
            public static readonly LocStr2 InputMinimum = Loc.Str2("ControlledTransport_InputMinimum", "Minimum: {0}/{1}", "'{0}' is selected value, '{0}' is actual capacity");
            public static readonly LocStr2 InputMaximum = Loc.Str2("ControlledTransport_InputMaximum", "Maximum: {0}/{1}", "'{0}' is selected value, '{0}' is actual capacity");
        }
    }


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
    }

    public class TransportControlView : StaticEntityInspectorBase<TransportControl>
    {
        private TransportControlInspector m_controller;
        private Txt txtMax;
        private Txt txtMin;
        private ISyncer<ProductProto> storageProducts;
        private ISyncer<Limits> storageLimits;
        private Slidder sldMin;
        private Slidder sldMax;
        private int configuration;
        private IconContainer icnProduct;

        private readonly List<ProductProto> EMPTY_LIST;
        public List<ProductProto> LAST_LIST;
        public Func<List<ProductProto>> EMPTY;
        public Func<List<ProductProto>> LAST;
        private Dictionary<ProductSlimId, ProductProto> protoCache;

        private class Limits
        {
            public int capacity = 1;
            public int minimum = 0;
            public int maximum = 1;

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                return obj is Limits l && l.capacity == capacity && l.minimum == minimum && l.maximum == maximum;
            }

            public override int GetHashCode()
            {
                return capacity.GetHashCode() ^ minimum.GetHashCode() ^ maximum.GetHashCode();
            }

            public static bool operator ==(Limits l1, Limits l2)
            {
                return l1.capacity == l2.capacity && l1.minimum == l2.minimum && l1.maximum == l2.maximum;
            }

            public static bool operator !=(Limits l1, Limits l2)
            {
                return l1.capacity != l2.capacity || l1.minimum != l2.minimum || l1.maximum != l2.maximum;
            }
        }

        private bool limitsChanged;
        private Limits limits = new Limits();
        private string strIcon;
        private string strIconLast;

        public TransportControlView(TransportControlInspector controller)
            : base(controller)
        {
            m_controller = controller;

            protoCache = new Dictionary<ProductSlimId, ProductProto>();
            foreach (var proto in m_controller.Context.ProtosDb.All<ProductProto>())
                protoCache[proto.SlimId] = proto;

            EMPTY_LIST = new List<ProductProto>();
            LAST_LIST = EMPTY_LIST;
            EMPTY = () => EMPTY_LIST;
            LAST = EMPTY;
        }

        protected override TransportControl Entity => m_controller.SelectedEntity;

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            base.AddCustomItems(itemContainer);

            var horizontal = Builder
                .NewStackContainer("all")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetWidth(400)
                .SetInnerPadding(new Offset(0,0,5,0))
                .AppendTo(itemContainer);

            icnProduct = Builder
                .NewIconContainer("item")
                .SetIcon(Style.Icons.Empty)
                .SetSize(new Vector2(40, 40))
                .AppendTo(horizontal);

            horizontal.AppendDivider(10, Style.Panel.Background);

            var vertical = Builder
                .NewStackContainer("vertical")
                .SetStackingDirection(StackContainer.Direction.BottomToTop)
                .AppendTo(horizontal);

            var maximumGrid = Builder
                .NewStackContainer("maximumGrid")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSize(new Vector2(350, 20))
                .SetBackground(ColorRgba.Black)
                .AppendTo(vertical);

            vertical.AppendDivider(10, ColorRgba.DarkDarkGray);

            var minimumGrid = Builder
                .NewStackContainer("minimumGrid")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSize(new Vector2(350, 20))
                .SetBackground(ColorRgba.Black)
                .AppendTo(vertical);

            txtMin = Builder
                .NewTxt("Minimum")
                .SetText(NewIds.Texts.InputMinimum.Format(0.ToString(), 0.ToString()))
                .SetSize(new Vector2(130, 20))
                .SetBackground(Style.QuantityBar.BackgroundColor)
                .AppendTo(minimumGrid);

            txtMax = Builder
                .NewTxt("Maximum")
                .SetSize(new Vector2(130, 20))
                .SetText(NewIds.Texts.InputMaximum.Format(1.ToString(), 1.ToString()))
                .SetBackground(Style.QuantityBar.BackgroundColor)
                .AppendTo(maximumGrid);

            var sldHolderMin = Builder
                .NewStackContainer("minimumGrid")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetBorderStyle(Style.Panel.Border)
                .AppendTo(minimumGrid);

            sldMin = Builder
                .NewSlider("minimumInputSlider")
                .SetCustomHandle(Builder.NewPanel("s").SetPivot(new Vector2(5,0)).SetBackground(ColorRgba.DarkGreen), 10)
                .SetSize(new Vector2(180, 20))
                .OnValueChange(MinimumCanged, MinimumCanged)
                .AppendTo(sldHolderMin);

            var sldHolderMax = Builder
                .NewStackContainer("minimumGrid")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetBorderStyle(Style.Panel.Border)
                .AppendTo(maximumGrid);

            sldMax = Builder
                .NewSlider("maximumInputSlider")
                .SetCustomHandle(Builder.NewPanel("s").SetPivot(new Vector2(5, 0)).SetBackground(ColorRgba.DarkRed), 10)
                .SetSize(new Vector2(180, 20))
                .OnValueChange(MaximumCanged, MaximumCanged)
                .AppendTo(sldHolderMax);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();
            storageProducts = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity?.NextProduct);
            storageLimits = updaterBuilder.CreateSyncer(() => new Limits
            {
                capacity = m_controller.SelectedEntity?.InputCapacity ?? 1,
                minimum = m_controller.SelectedEntity?.MinimumTroughput ?? 0,
                maximum = m_controller.SelectedEntity?.MaximumTroughput ?? 1
            });

            AddUpdater(updaterBuilder.Build());
        }

        private void MinimumCanged(float v)
        {
            if (configuration > 0) return;
            m_controller.SelectedEntity.MinimumTroughput = Mathf.Max(0, Mathf.Min(limits.maximum - 1, (int)((limits.maximum - 1) * sldMin.Value)));
        }

        private void MaximumCanged(float v)
        {
            if (configuration > 0) return;
            m_controller.SelectedEntity.MaximumTroughput = Mathf.Max(1, Mathf.Min(limits.capacity, (int)(limits.capacity * sldMax.Value)));
            if (m_controller.SelectedEntity.MinimumTroughput >= m_controller.SelectedEntity.MaximumTroughput)
                m_controller.SelectedEntity.MinimumTroughput =  m_controller.SelectedEntity.MaximumTroughput - 1;
        }

        public override void SyncUpdate(GameTime gameTime)
        {
            base.SyncUpdate(gameTime);

            if (storageLimits.HasChanged)
            {
                limitsChanged = true;
                limits = storageLimits.GetValueAndReset();
            }

            if (storageProducts.HasChanged)
            {
                strIcon = storageProducts.GetValueAndReset()?.IconPath ?? Style.Icons.Empty;
            }
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            // Values
            if (limitsChanged)
            {
                limitsChanged = false;
                configuration++;
                sldMax.SetValue(Mathf.Max(0, Mathf.Min(1, limits.maximum / (float)limits.capacity)));
                sldMin.SetValue(Mathf.Max(0, Mathf.Min(1, limits.minimum / Mathf.Max(1, (float)(limits.maximum - 1)))));
                configuration--;
                DisplayLimits();
            }

            // Icons
            if (strIconLast != strIcon)
            {
                strIconLast = strIcon;
                icnProduct.SetIcon(strIcon);
            }
        }

        private void DisplayLimits()
        {
            txtMin.SetText(NewIds.Texts.InputMinimum.Format(m_controller.SelectedEntity?.MinimumTroughput.ToString() ?? "0", (m_controller.SelectedEntity?.MaximumTroughput - 1).ToString()));
            txtMax.SetText(NewIds.Texts.InputMaximum.Format(m_controller.SelectedEntity?.MaximumTroughput.ToString() ?? "1", limits.capacity.ToString()));
        }
    }
}
