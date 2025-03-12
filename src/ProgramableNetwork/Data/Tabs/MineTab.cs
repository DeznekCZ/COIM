using Mafi;
using Mafi.Core.Products;
using Mafi.Core.World.Entities;
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
    public class MineTab : StackContainer/*, IRefreshable*/
    {
        private readonly UiBuilder m_builder;
        private readonly AMDataBandChannel m_fieldId;
        private readonly Antena m_module;
        private readonly Func<WorldMapMine, bool> m_filter;
        private readonly ItemDetailWindowView m_window;
        private readonly AntenaInspector m_inspector;
        private readonly Action m_refresh;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;
        private Btn m_btnClear;
        private ProtoPicker<MineInstanceProto> m_protoPicker;

        public MineTab(UiBuilder builder, Antena module, AMDataBandChannel fieldId, Func<Antena, WorldMapMine, bool> filter,
            ItemDetailWindowView parentWindow, AntenaInspector inspector, Action refresh)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_fieldId = fieldId;
            m_module = module;
            m_filter = (proto) => filter.Invoke(m_module, proto);
            m_window = parentWindow;
            m_inspector = inspector;
            m_refresh = refresh;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetSize(60, 40)
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(m_builder.Style.Icons.Empty)
                .OnClick(FindProduct)
                .ToolTip(m_inspector, () => MineInstanceProto.GetStrings(m_fieldId.WorldMapMine).Name.TranslatedString)
                .AppendTo(m_btnPreviewHolder);

            m_btnClear = m_builder
                .NewBtnGeneral("clear_" + DateTime.Now.Ticks)
                .SetSize(20, 40)
                .SetText("X")
                .OnClick(() => {
                    m_fieldId.WorldMapMine = null;
                    m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                    m_btnClear.SetVisibility(false);
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
                m_protoPicker = new ProtoPicker<MineInstanceProto>(
                    (product) =>
                    {
                        m_fieldId.WorldMapMine = product.Mine;
                        m_window.OnHide -= protoPicker_Hide;
                        m_protoPicker.Hide();
                        try
                        {
                            m_btnPreviewHolder.ClearAndDestroyAll();
                            m_btnPreview = new Btn(m_builder, "picker_" + DateTime.Now.Ticks)
                                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                                .SetSize(40, 40)
                                .SetIcon(m_fieldId.WorldMapMine.Prototype.IconPath)
                                .ToolTip(m_inspector, () => MineInstanceProto.GetStrings(m_fieldId.WorldMapMine).Name.TranslatedString)
                                .OnClick(FindProduct)
                                .AppendTo(m_btnPreviewHolder);

                            m_btnClear = m_builder
                                .NewBtnGeneral("clear_" + DateTime.Now.Ticks)
                                .SetSize(20, 40)
                                .SetText("X")
                                .OnClick(() => {
                                    m_fieldId.WorldMapMine = null;
                                    m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                                    m_btnClear.SetVisibility(false);
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

            m_protoPicker.SetVisibleProtos(m_module.Context.EntitiesManager
                .GetAllEntitiesOfType<WorldMapMine>()
                .Where(p => p.IsOwnedByPlayer)
                .Where(m_filter)
                .Select(p => new MineInstanceProto(p))
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
            if (m_fieldId.WorldMapMine is null)
            {
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                m_btnClear.SetVisibility(false);
                m_refresh();
                return;
            }

            if (!m_fieldId.WorldMapMine.IsOwnedByPlayer || !m_filter(m_fieldId.WorldMapMine))
            {
                m_fieldId.WorldMapMine = null;
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                m_btnClear.SetVisibility(false);
                m_refresh();
                return;
            }

            m_btnPreview.SetIcon(m_fieldId.WorldMapMine.Prototype.IconPath);
            m_btnClear.SetVisibility(true);
            m_refresh();
        }
    }
}