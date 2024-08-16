using Mafi;
using Mafi.Core.Products;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Linq;
using UnityEngine;

namespace MultiplayerContracts
{
    public class SelectProduct : StackContainer
    {
        private readonly UiBuilder m_builder;
        private readonly Func<ProductProto, bool> m_filter;
        private readonly Action m_refresh;
        private readonly ItemDetailWindowView m_window;
        private readonly InspectorContext m_inspectorContext;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;
        private ProductProto m_product;
        private ProtoPicker<ProductProto> m_protoPicker;

        public ProductProto Product => m_product;

        public SelectProduct(string name, UiBuilder builder, Action refresh, ItemDetailWindowView parentWindow, InspectorContext inspectorContext, Func<ProductProto, bool> filter = null)
            : base(builder, name + "_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspectorContext = inspectorContext;
            m_filter = filter ?? ((product) => true);

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetSize(new Vector2(40, 40))
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(new Vector2(40, 40))
                .SetIcon(m_builder.Style.Icons.Empty)
                .OnClick(FindProduct)
                .AppendTo(m_btnPreviewHolder);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(40);

            Refresh();
        }

        private void FindProduct()
        {
            if (m_protoPicker == null)
            {
                m_protoPicker = new ProtoPicker<ProductProto>(
                    (product) =>
                    {
                        m_product = product;
                        m_window.OnHide -= protoPicker_Hide;
                        m_protoPicker.Hide();
                        try
                        {
                            m_btnPreviewHolder.ClearAndDestroyAll();
                            m_btnPreview = m_builder
                                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                                .SetSize(new Vector2(40, 40))
                                .SetIcon(m_builder.Style.Icons.Empty)
                                .OnClick(FindProduct)
                                .AppendTo(m_btnPreviewHolder);
                        }
                        catch (Exception e)
                        {
                            Log.Warning(e.Message + "\n" + e.StackTrace);
                        }
                        m_refresh();
                        Refresh();
                    },
                    (product) => new Mafi.Localization.LocStrFormatted($"Tradeable: {GetAmount(product)}"),
                    false);

                m_window.SetupInnerWindowWithButton(m_protoPicker, m_btnPreviewHolder, m_btnPreview, () => {
                    try {
                        m_btnPreviewHolder.ClearAndDestroyAll();
                        m_btnPreview = m_builder
                            .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                            .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                            .SetSize(new Vector2(40, 40))
                            .SetIcon(m_builder.Style.Icons.Empty)
                            .OnClick(FindProduct)
                            .AppendTo(m_btnPreviewHolder);
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e.Message + "\n" + e.StackTrace);
                    }
                }, () => { });

                m_protoPicker.BuildIfNeeded(m_builder, m_window);
                m_protoPicker.SetSize(new Vector2(400, 400));
                m_protoPicker.SetTitle(Tr.ProductsToFilter);
            }

            m_protoPicker.SetVisibleProtos(m_inspectorContext.ProtosDb
                .All<ProductProto>()
                .Where(p => p.IsAvailable && p.CanBeLoadedOnTruck)
                .Where(m_filter)
                .ToList());

            m_window.OnHide += protoPicker_Hide;
            m_protoPicker.Show();
        }

        private void protoPicker_Hide()
        {
            try
            {
                m_protoPicker.Hide();
            }
            catch (Exception e)
            {
                Log.Warning(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                m_window.OnHide -= protoPicker_Hide;
            }
        }

        public void Refresh()
        {
            if (m_product != null)
                m_btnPreview.SetIcon(m_product.IconPath);
            else
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
        }

        private long GetAmount(SelectProduct m_product)
        {
            if (m_product.Product == null)
                return 0;

            return GetAmount(m_product.Product);
        }

        private long GetAmount(ProductProto m_product)
        {
            var stats = m_inspectorContext.AssetsManager
                .GetAvailableQuantityForRemoval(m_product);

            return stats.Value;
        }
    }
}