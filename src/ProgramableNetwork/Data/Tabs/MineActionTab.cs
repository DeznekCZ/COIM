using Mafi;
using Mafi.Core;
using Mafi.Core.World.Entities;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using static ProgramableNetwork.AMDataBandChannel;

namespace ProgramableNetwork
{
    public class MineActionTab : ButtonIcon/*, IRefreshable*/
    {
        private readonly AMDataBandChannel m_fieldId;
        private readonly AntenaInspector m_inspector;
        private readonly Window m_window;
        private ProtoPickerPopup<MineActionProto> m_protoPicker;

        public MineActionTab(UiContext uiContext, Antena module, AMDataBandChannel fieldId, Func<Antena, WorldMapMine, bool> filter,
            Window parentWindow, AntenaInspector antenaInspector)
            : base(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png)
        {
            m_fieldId = fieldId;
            m_inspector = antenaInspector;
            m_window = parentWindow;
            m_window.OnCloseStart += ParentWindow_OnCloseStart;

            this.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
            this.OnClick(FindProduct);
            this.Tooltip(Tr.Empty);

            m_protoPicker = new ProtoPickerPopup<MineActionProto>(
                optionsProvider: GetOperationTypes,
                optionViewFactory: (item) => new ButtonIcon(item.IconPath).Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE),
                onOptionSelected: (product) =>
                {
                    m_fieldId.Operation = product.Value;
                    m_protoPicker.Hide();
                    Refresh();
                },
                button: this,
                title: new Mafi.Localization.LocStrFormatted("Pick an operation"),
                config: new ProtoPickerConfig
                {
                    ItemSize = new UnityEngine.Vector2(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE),
                    ItemsPerRow = 6
                },
                orderAlphabetically: false,
                searchable: false
            );

            Refresh();
        }

        private void ParentWindow_OnCloseStart(Window obj)
        {
            m_protoPicker.Close();
        }

        protected override void OnDetached()
        {
            m_window.OnCloseStart -= ParentWindow_OnCloseStart;
            base.OnDetached();
        }

        private void FindProduct()
        {
            m_protoPicker.Open(m_window);
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

        public void Refresh()
        {
            if (m_fieldId.Operation == 0)
            {
                Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                this.Tooltip(Tr.Empty);
                return;
            }

            Icon.Value(MineActionProto.GetIconPath(m_fieldId.Operation, m_fieldId));
            this.Tooltip(new Mafi.Localization.LocStrFormatted(MineActionProto.GetName(m_fieldId.Operation, m_fieldId)));
        }
    }
}