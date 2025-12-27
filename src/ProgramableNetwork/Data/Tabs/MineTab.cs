using Mafi;
using Mafi.Core.Syncers;
using Mafi.Core.World.Entities;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

namespace ProgramableNetwork.Ui
{
    public class MineTab : ButtonIcon/*, IRefreshable*/
    {
        public MineTab(UiContext uiContext, Antena module, AMDataBandChannel fieldId, Fix32 distanceBoost,
            Window parentWindow, Ui.AntenaInspector inspector)
            : base(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png)
        {
            this.Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE * 2);
            Icon.Size(Percent.Eighty, Percent.Eighty);

            var protoPicker = new ProtoPickerPopup<MineInstanceProto>(
                optionsProvider: () => module.Context.EntitiesManager
                    .GetAllEntitiesOfType<WorldMapMine>()
                    .Where(p => p.IsOwnedByPlayer)
                    .Select(p => new MineInstanceProto(p, fieldId))
                    .ToList(),
                optionViewFactory: (product) => new ButtonRow()
                                                    .Gap(5.px())
                                                    .AddAndReturn(new Icon(product.IconPath))
                                                        .Width(Sizes.BLOCK_SIZE * 2)
                                                        .Parent.As<ButtonRow>().Value
                                                    .AddAndReturn(new Label(product.Strings.Name))
                                                        .Width(Sizes.BLOCK_SIZE * 6)
                                                        .Parent.As<ButtonRow>().Value
                                                    .Tooltip(MineInstanceProto.GetStrings(fieldId.WorldMapMine, fieldId).DescShort)
                                                    .Height(Sizes.BLOCK_SIZE * 2)
                                                    .Width(Sizes.BLOCK_SIZE * 8)
                                                    .AsProtoPickerOptionButton(),
                onOptionSelected: (product) => fieldId.WorldMapMine = product.Mine,
                button: this,
                title: LocStrFormatted.Empty,
                config: new ProtoPickerConfig
                {
                    ItemSize = new UnityEngine.Vector2(Sizes.BLOCK_SIZE * 8, Sizes.BLOCK_SIZE * 2),
                    ItemsPerRow = 1
                },
                orderAlphabetically: true,
                searchable: true
            );

            this.Observe(() => fieldId.WorldMapMine)
                .Do((mine) =>
                {
                    if (mine is null)
                    {
                        Icon.Empty();
                        return;
                    }

                    Icon.Value(fieldId.WorldMapMine.Prototype.IconPath);
                    Icon.Tooltip(MineInstanceProto.GetStrings(fieldId.WorldMapMine, fieldId).Name);
                });
        }
    }
}