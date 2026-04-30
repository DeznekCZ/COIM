using Mafi;
using Mafi.Core.Syncers;
using Mafi.Core.World;
using Mafi.Core.World.Entities;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System.Collections.Generic;
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
                optionsProvider: () =>
                {
                    var options = new List<MineInstanceProto>();
                    // Mines owned by the player
                    options.AddRange(module.Context.EntitiesManager
                        .GetAllEntitiesOfType<WorldMapMine>()
                        .Where(p => p.IsOwnedByPlayer)
                        .Select(p => new MineInstanceProto(p, fieldId)));
                    // Player's main ship — selectable as a source for ship-* operations.
                    var ship = module.Context.EntitiesManager
                        .GetAllEntitiesOfType<BattleShip>()
                        .FirstOrDefault(s => !s.IsDestroyed);
                    if (ship != null)
                    {
                        options.Add(new MineInstanceProto(ship, fieldId));
                    }
                    return options;
                },
                optionViewFactory: (product) => new ButtonRow()
                                                    .Gap(5.px())
                                                    .AddAndReturn(new Icon(product.IconPath))
                                                        .Width(Sizes.BLOCK_SIZE * 2)
                                                        .Parent.As<ButtonRow>().Value
                                                    .AddAndReturn(new Label(product.Strings.Name))
                                                        .Width(Sizes.BLOCK_SIZE * 6)
                                                        .Parent.As<ButtonRow>().Value
                                                    .Tooltip(product.Strings.DescShort)
                                                    .Height(Sizes.BLOCK_SIZE * 2)
                                                    .Width(Sizes.BLOCK_SIZE * 8)
                                                    .AsProtoPickerOptionButton(),
                onOptionSelected: (product) =>
                {
                    int slot = currentSlot(fieldId);
                    if (slot < 0) {
                        // Defensive — channel not currently in the band's redirected list.
                        return;
                    }
                    var sourceId = product.IsShip ? product.Ship?.Id : product.Mine?.Id;
                    uiContext.InputScheduler.ScheduleInputCmd(new AntenaChannelSetAmSourceCmd(
                        module.Id, slot, sourceId));
                },
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

            // Observe both bindings — only one is set at a time.
            this.Observe(() => fieldId.WorldMapMine)
                .Observe(() => fieldId.BattleShip)
                .Do((mine, ship) =>
                {
                    if (mine != null)
                    {
                        Icon.Value(mine.Prototype.IconPath);
                        Icon.Tooltip(MineInstanceProto.GetStrings(mine, fieldId).Name);
                        return;
                    }
                    if (ship != null)
                    {
                        Icon.Value(ship.Prototype.Graphics.IconPath);
                        Icon.Tooltip(MineInstanceProto.GetShipStrings(ship).Name);
                        return;
                    }
                    Icon.Empty();
                });
        }

        private static int currentSlot(AMDataBandChannel channel)
        {
            int i = 0;
            foreach (var c in channel.OriginalDataBand.Channels)
            {
                if (object.ReferenceEquals(c, channel)) { return i; }
                i++;
            }
            return -1;
        }
    }
}