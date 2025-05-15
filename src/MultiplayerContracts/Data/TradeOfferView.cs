using Mafi;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using static Mafi.Unity.Assets.Unity;

namespace MultiplayerContracts.Data
{
    internal class TradeOfferView : PanelRow
    {
        public TradeOfferView(long offer, ContractParameters contract, Action removeOffer, TradeType tradeType, IAssetTransactionManager assetsManager)
            : base(gap: 5.px(), noBolts: true)
        {
            this.ClassRemove(Cls.panel);
            this.Class(Cls.group);

            Body.AddAndReturn(new Icon())
                .Large()
                .Observe((icon) => (icon, contract.Supply.Product))
                .Do(entry => entry.icon.Value(entry.Product));

            Body.AddAndReturn(new Column(gap: 5.px())
            {
                new Label().Height(20)
                    .With(l => l.Observe(() => contract.Supply.Quantity).Do(quantity => l.Value(quantity.Value.ToFix32()))),
                new Label().Height(11)
                    .With(l => l.Observe(() => assetsManager.GetAvailableQuantityForRemoval(contract.Supply.Product)).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
            });

            Body.AddAndReturn(new Icon(UserInterface.General.ArrowRight_svg)
                            .Width(36.px())
                            .Height(36.px())
                        );

            Body.AddAndReturn(new Icon())
                .Large()
                .Observe((icon) => (icon, contract.Demand.Product))
                .Do(entry => entry.icon.Value(entry.Product));

            Body.AddAndReturn(new Column(gap: 5.px())
            {
                new Label().Height(20)
                    .With(l => l.Observe(() => contract.Demand.Quantity).Do(quantity => l.Value(quantity.Value.ToFix32()))),
                new Label().Height(11)
                    .With(l => l.Observe(() => assetsManager.GetAvailableQuantityForRemoval(contract.Demand.Product)).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
            });

            Body.AddAndReturn(new ButtonIcon(Button.Unity, UserInterface.Toolbar.TradeClipped_svg)
                .NoShrink()
                .MarginLeft(2.pt())
                .CustomClickSound(UserInterface.Audio.MoneyAction_prefab)
                .ObserveEnabledWithReason(() =>
                {
                    bool enough = assetsManager.GetAvailableQuantityForRemoval(contract.Demand.Product) >= contract.Demand.Quantity;
                    return new Mafi.Core.Utils.BoolWithReason(enough,
                        enough
                            ? $"{Tr.BuyPrefix}: {contract.Demand.FormatNumberAndUnitOnly()}".AsLoc()
                            : Tr.TradeStatus__CantAfford.AsFormatted
                    );
                }, "".AsLoc())
                .OnClick(() =>
                {
                    assetsManager.RemoveAsMuchAs(contract.Demand, Mafi.Core.Products.DestroyReason.QuickTrade);
                    removeOffer();
                })
            );
        }
    }
}