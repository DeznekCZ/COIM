using Mafi;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Entities;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Security.Policy;
using System.Threading.Tasks;
using static Mafi.Unity.Assets.Unity;

namespace MultiplayerContracts.Data.Entries
{
    internal class TradeOfferViewBuy : PanelRow
    {
        private bool m_tradeInProgress;

        public TradeOfferViewBuy(MultiplayerTradeDock tradeDock, long offer, ContractParameters contract, Action removeOffer, IAssetTransactionManager assetsManager, UiContext context)
            : base(gap: 5.px(), noBolts: true)
        {
            this.ClassRemove(Cls.panel);
            this.ClassRemove(Cls.shadowAll);
            this.Class(Cls.group);

            Body.AddAndReturn(new Icon())
                .Large()
                .Observe((icon) => (icon, contract.Demand.Product))
                .Do(entry => entry.icon.Value(entry.Product));

            Body.AddAndReturn(new Column(gap: 5.px())
            {
                new Label().Height(20).FontBold()
                    .With(l => l.Observe(() => contract.Demand.Quantity).Do(quantity => l.Value(quantity.Value.ToFix32()))),
                new Label().Height(11)
                    .With(l => l.Observe(() => assetsManager.GetAvailableQuantityForRemoval(contract.Demand.Product)).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
            });

            Body.AddAndReturn(new Icon(UserInterface.General.ArrowRight_svg)
                            .Width(24.px())
                            .Height(24.px())
                        );

            Body.AddAndReturn(new Icon())
                .Large()
                .Observe((icon) => (icon, contract.Supply.Product))
                .Do(entry => entry.icon.Value(entry.Product));

            Body.AddAndReturn(new Column(gap: 5.px())
            {
                new Label().Height(20).FontBold()
                    .With(l => l.Observe(() => contract.Supply.Quantity).Do(quantity => l.Value(quantity.Value.ToFix32()))),
                new Label().Height(11)
                    .With(l => l.Observe(() => assetsManager.GetAvailableQuantityForRemoval(contract.Supply.Product)).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
            });

            Body.AddAndReturn(new ButtonIcon(Button.Unity, UserInterface.Toolbar.TradeClipped_svg)
                .NoShrink()
                .MarginLeft(2.pt())
                .ObserveEnabledWithReason(() =>
                {
                    if (m_tradeInProgress)
                        return new Mafi.Core.Utils.BoolWithReason(false, "Trade in progress".AsLoc());

                    Quantity quantity = tradeDock.GetQuantity();
                    if (contract.Demand.Quantity > tradeDock.Prototype.Capacity - quantity)
                        return new Mafi.Core.Utils.BoolWithReason(
                            false, $"{Tr.EntityStatus__FullStorage}: ({tradeDock.Prototype.Capacity})".AsLoc());

                    bool enough = assetsManager.GetAvailableQuantityForRemoval(contract.Demand.Product) >= contract.Demand.Quantity;
                    return new Mafi.Core.Utils.BoolWithReason(enough,
                        enough
                            ? $"{Tr.SellPrefix}: {contract.Demand.FormatNumberAndUnitOnly()}\n{Tr.BuyPrefix}: {contract.Supply.FormatNumberAndUnitOnly()}\n".AsLoc()
                            : Tr.TradeStatus__CantAfford.AsFormatted
                    );
                }, "".AsLoc())
                .OnClick(() =>
                {
                    m_tradeInProgress = true;
                    var much = assetsManager.RemoveAsMuchAs(contract.Demand, Mafi.Core.Products.DestroyReason.QuickTrade);
                    if (much != contract.Demand.Quantity)
                    {
                        tradeDock.AddProduct(much.Of(contract.Demand.Product));
                        context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
                        return;
                    }

                    MultiplayerTradeManager.TakeContract(tradeDock.Address, tradeDock.Authorization, offer)
                        .ContinueWith((result) =>
                        {
                            if (result.Status == TaskStatus.RanToCompletion && result.Result)
                            {
                                tradeDock.AddProduct(contract.Supply);
                                removeOffer();
                                RemoveFromHierarchy();
                                context.AudioDb.GetSharedAudioUi(UserInterface.Audio.MoneyAction_prefab);
                            }
                            else
                            {
                                tradeDock.AddProduct(contract.Demand);
                                context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
                                m_tradeInProgress = false;
                            }
                        });
                })
            );
        }
    }
}