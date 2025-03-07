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

namespace ProgramableNetwork
{
    public class ProductTab : StackContainer/*, IRefreshable*/
    {
        private readonly UiBuilder m_builder;
        private readonly string m_fieldId;
        private readonly Module m_module;
        private readonly Func<ProductProto, bool> m_filter;
        private readonly Action m_refresh;
        private readonly ItemDetailWindowView m_window;
        private readonly InspectorContext m_inspectorContext;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;
        private ProtoPicker<ProductProto> m_protoPicker;

        public ProductTab(UiBuilder builder, Module module, string fieldId, Func<Module, ProductProto, bool> filter,
            Action refresh, ItemDetailWindowView parentWindow, InspectorContext inspectorContext)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_fieldId = fieldId;
            m_module = module;
            m_filter = (proto) => filter.Invoke(m_module, proto);
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspectorContext = inspectorContext;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetSize(40, 40)
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(40, 40)
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
                        m_module.Field[m_fieldId] = Fix32.FromRaw(product.SlimId.Value);
                        m_window.OnHide -= protoPicker_Hide;
                        m_protoPicker.Hide();
                        try
                        {
                            m_btnPreviewHolder.ClearAndDestroyAll();
                            m_btnPreview = new Btn(m_builder, "picker_" + DateTime.Now.Ticks)
                                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                                .SetSize(40, 40)
                                .SetIcon(m_builder.Style.Icons.Empty)
                                .OnClick(FindProduct)
                                .AppendTo(m_btnPreviewHolder);
                        }
                        catch (Exception)
                        {
                            // gui issue
                        }
                        m_refresh();
                    },
                    (product) => product.Strings.DescShort,
                    false);

                m_protoPicker.BuildIfNeeded(m_builder);
                m_protoPicker.SetSize(400, 400);
                m_protoPicker.SetTitle(Tr.ProductsToFilter);

                m_window.SetupInnerWindowWithButton(m_protoPicker, m_btnPreviewHolder, m_btnPreview, () => {
                    try {
                        m_btnPreviewHolder.ClearAndDestroyAll();
                        m_btnPreview = new Btn(m_builder, "picker_" + DateTime.Now.Ticks)
                            .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                            .SetSize(40, 40)
                            .SetIcon(m_builder.Style.Icons.Empty)
                            .OnClick(FindProduct)
                            .AppendTo(m_btnPreviewHolder);
                    }
                    catch (Exception)
                    {
                        // gui issue
                    }
                }, () => { });
            }

            m_protoPicker.SetVisibleProtos(m_inspectorContext.ProtosDb
                .All<ProductProto>()
                .Where(p => p.IsAvailable)
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
            catch (Exception)
            {
                // ignore
            }
            finally
            {
                m_window.OnHide -= protoPicker_Hide;
            }
        }

        public void Refresh()
        {
            int slimId = m_module.Field[m_fieldId, Fix32.Zero].RawValue;

            if (slimId == 0)
            {
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                return;
            }

            ProductProto foundProduct = m_module.Context.ProtosDb
                .Filter<ProductProto>(product => product.SlimId.Value == slimId)
                .FirstOrDefault();

            if (foundProduct == null)
            {
                m_module.Field[m_fieldId] = Fix32.Zero;
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                return;
            }

            m_btnPreview.SetIcon(foundProduct.IconPath);
        }
    }
}