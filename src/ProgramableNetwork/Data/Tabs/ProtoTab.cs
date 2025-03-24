using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
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
    public class ProtoTab<T> : StackContainer/*, IRefreshable*/
        where T : EntityProto, IProtoWithIcon
    {
        private readonly UiBuilder m_builder;
        private readonly string m_fieldId;
        private readonly Module m_module;
        private readonly Func<T, bool> m_filter;
        private readonly Action m_refresh;
        private readonly ItemDetailWindowView m_window;
        private readonly InspectorContext m_inspectorContext;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;
        private Btn m_btnClear;
        private ProtoPicker<T> m_protoPicker;

        public ProtoTab(UiBuilder builder, Module module, string fieldId, Func<Module, T, bool> filter,
            Action refresh, ItemDetailWindowView parentWindow, InspectorContext inspectorContext)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_fieldId = fieldId;
            m_module = module;
            m_filter = (e) => filter.Invoke(m_module, e);
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

            m_btnClear = m_builder
                .NewBtnGeneral("clear_" + DateTime.Now.Ticks)
                .SetSize(20, 40)
                .SetText("X")
                .OnClick(() => {
                    m_module.Field[m_fieldId] = Fix32.Zero;
                    m_module.Field[m_fieldId, false] = "";
                    m_refresh();
                })
                .AppendTo(m_btnPreviewHolder);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(40);
            this.SetWidth(60);

            Refresh();
        }

        private void FindProduct()
        {
            if (m_protoPicker == null)
            {
                m_protoPicker = new ProtoPicker<T>(
                    (product) =>
                    {
                        m_module.Field[m_fieldId] = FixSavedGames.GetPrototypeString(product.Id.Value);
                        m_module.Field[m_fieldId, false] = product.Id.Value;
                        m_window.OnHide -= protoPicker_Hide;
                        m_protoPicker.Hide();
                        try
                        {
                            m_btnPreviewHolder.ClearAndDestroyAll();
                            m_btnPreview = new Btn(m_builder, "picker_" + DateTime.Now.Ticks)
                                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                                .SetSize(40, 40)
                                .SetIcon(product.IconPath)
                                .OnClick(FindProduct)
                                .AppendTo(m_btnPreviewHolder);

                            m_btnClear = m_builder
                                .NewBtnGeneral("clear_" + DateTime.Now.Ticks)
                                .SetSize(20, 40)
                                .SetText("X")
                                .OnClick(() => {
                                    m_module.Field[m_fieldId] = Fix32.Zero;
                                    m_module.Field[m_fieldId, false] = "";
                                    m_refresh();
                                })
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
                m_protoPicker.SetTitle(Tr.SelectVehicle_Title);

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
                .All<T>()
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
            string slimId = m_module.Field[m_fieldId, ""];

            if (string.IsNullOrEmpty(slimId))
            {
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                m_btnClear.SetVisibility(false);
                return;
            }

            T foundProduct = m_module.Context.ProtosDb
                .Get<T>(new Proto.ID(slimId)).ValueOrNull;

            if (foundProduct == null)
            {
                m_module.Field[m_fieldId] = Fix32.Zero;
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                m_btnClear.SetVisibility(false);
                return;
            }

            m_btnPreview.SetIcon(foundProduct.IconPath);
            m_btnClear.SetVisibility(true);
        }
    }
}