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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ProgramableNetwork.AMDataBandChannel;

namespace ProgramableNetwork
{
    public class MineActionTab : StackContainer/*, IRefreshable*/
    {
        private readonly UiBuilder m_builder;
        private readonly AMDataBandChannel m_fieldId;
        private readonly ItemDetailWindowView m_window;
        private readonly AntenaInspector m_inspector;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;
        private ProtoPicker<MineActionProto> m_protoPicker;

        public MineActionTab(UiBuilder builder, Antena module, AMDataBandChannel fieldId, Func<Antena, WorldMapMine, bool> filter,
            ItemDetailWindowView parentWindow, AntenaInspector antenaInspector)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_fieldId = fieldId;
            m_window = parentWindow;
            m_inspector = antenaInspector;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetSize(40, 40)
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(m_builder.Style.Icons.Empty)
                .ToolTip(antenaInspector, () => MineActionProto.GetName(m_fieldId.Operation, m_fieldId))
                .OnClick(FindProduct)
                .AppendTo(m_btnPreviewHolder);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(40);
            this.SetWidth(40);

            Refresh();
        }

        private void FindProduct()
        {
            if (m_protoPicker == null)
            {
                m_protoPicker = new ProtoPicker<MineActionProto>(
                    (product) =>
                    {
                        m_fieldId.Operation = product.Value;
                        m_window.OnHide -= protoPicker_Hide;
                        m_protoPicker.Hide();
                        try
                        {
                            m_btnPreviewHolder.ClearAndDestroyAll();
                            m_btnPreview = m_builder
                                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                                .SetSize(40, 40)
                                .SetIcon(product.IconPath)
                                .ToolTip(m_inspector, () => MineActionProto.GetName(m_fieldId.Operation, m_fieldId))
                                .OnClick(FindProduct)
                                .AppendTo(m_btnPreviewHolder);
                        }
                        catch (Exception)
                        {
                            // gui issue
                        }
                    },
                    (product) => product.Strings.DescShort,
                    false);

                m_protoPicker.BuildIfNeeded(m_builder);
                m_protoPicker.SetSize(400, 400);
                m_protoPicker.SetTitle(Tr.ProductsToFilter);

                m_window.SetupInnerWindowWithButton(m_protoPicker, m_btnPreviewHolder, m_btnPreview, () => {
                    m_btnPreview.SetParent(m_btnPreviewHolder);
                }, () => { });
            }

            m_protoPicker.SetVisibleProtos(GetOperationTypes().ToList());

            m_window.OnHide += protoPicker_Hide;
            m_protoPicker.Show();
        }

        private IEnumerable<MineActionProto> GetOperationTypes()
        {
            Type am = typeof(AMOperation);
            foreach (AMOperation item in Enum.GetValues(am))
            {
                yield return new MineActionProto(item, m_fieldId);
            }
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
            if (m_fieldId.Operation == 0)
            {
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                return;
            }

            m_btnPreview.SetIcon(MineActionProto.GetIconPath(m_fieldId.Operation, m_fieldId));
        }
    }
}