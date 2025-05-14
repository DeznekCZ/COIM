using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Core.World.Entities;
using Mafi.Localization;
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
        public MineActionTab(UiContext uiContext, Antena module, AMDataBandChannel fieldId, Func<Antena, WorldMapMine, bool> filter,
            Window parentWindow, AntenaInspector antenaInspector)
            : base(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png)
        {
            this.Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE * 2);
            Icon.Size(Percent.Eighty, Percent.Eighty);

            this.Observe(() => fieldId.Operation)
                .Do(operation => {
                    if (fieldId.Operation == 0)
                    {
                        Icon.Empty();
                        return;
                    }

                    Icon.Value(MineActionProto.GetIconPath(fieldId.Operation, fieldId));
                    Icon.Tooltip(MineActionProto.GetName(fieldId.Operation, fieldId));
                });

            var protoPicker = new ProtoPickerPopup<MineActionProto>(
                optionsProvider: () => GetOperationTypes(fieldId),
                optionViewFactory: (item) => new ButtonRow()
                                                .Gap(5.px())
                                                .AddAndReturn(new Icon(item.IconPath))
                                                    .Width(Sizes.BLOCK_SIZE * 2)
                                                    .Parent.As<ButtonRow>().Value
                                                .AddAndReturn(new Label(item.Strings.Name)
                                                    .Width(Sizes.BLOCK_SIZE * 6))
                                                    .Parent.As<ButtonRow>().Value
                                                .Tooltip(item.Strings.DescShort)
                                                .Height(Sizes.BLOCK_SIZE * 2)
                                                .Width(Sizes.BLOCK_SIZE * 8),
                onOptionSelected: (product) => fieldId.Operation = product.Value,
                button: this,
                title: LocStrFormatted.Empty,
                config: new ProtoPickerConfig
                {
                    ItemSize = new UnityEngine.Vector2(Sizes.BLOCK_SIZE * 8, Sizes.BLOCK_SIZE * 2),
                    ItemsPerRow = 1
                },
                orderAlphabetically: false,
                searchable: false
            );
        }

        private IEnumerable<MineActionProto> GetOperationTypes(AMDataBandChannel fieldId)
        {
            Type am = typeof(AMOperation);
            foreach (AMOperation item in Enum.GetValues(am))
            {
                yield return new MineActionProto(item, fieldId);
            }
        }
    }
}