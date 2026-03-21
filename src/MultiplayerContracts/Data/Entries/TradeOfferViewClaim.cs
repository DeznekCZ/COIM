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
using System.Threading.Tasks;
using static Mafi.Unity.Assets.Unity;

namespace MultiplayerContracts.Data
{
    internal class TradeOfferViewClaim : Row
    {
        private bool m_tradeInProgress;

        public TradeOfferViewClaim(MultiplayerTradeDock tradeDock, long offer, ContractParameters contract, Action removeOffer, IAssetTransactionManager assetsManager, UiContext context)
            : base(gap: 5.px())
        {
            this.Class(Cls.group);
            this.Padding(10);

            this.AddAndReturn(new Icon())
                .Large()
                .Observe((icon) => (icon, contract.Demand.Product))
                .Do(entry => entry.icon.Value(entry.Product));

            this.AddAndReturn(new Column(gap: 5.px())
            {
                new Label().Height(20).FontBold()
                    .With(l => l.Observe(() => contract.Demand.Quantity).Do(quantity => l.Value(quantity.Value.ToFix32()))),
                new Label().Height(11)
                    .With(l => l.Observe(() => assetsManager.GetAvailableQuantityForRemoval(contract.Demand.Product)).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
            });

            this.AddAndReturn(new ButtonIcon(Button.Unity, UserInterface.EntityIcons.Storage_svg)
                .NoShrink()
                .MarginLeft(2.pt())
                .ObserveEnabledWithReason(() =>
                {
                    if (m_tradeInProgress) {
						return new Mafi.Core.Utils.BoolWithReason(false, MpTr.ClaimingProgress);
					}

					Quantity quantity = tradeDock.GetQuantity();
                    if (contract.Demand.Quantity > tradeDock.Prototype.Capacity - quantity) {
						return new Mafi.Core.Utils.BoolWithReason(
							false, $"{Tr.EntityStatus__FullStorage}: ({quantity}/{tradeDock.Prototype.Capacity})".AsLoc());
					}

					return new Mafi.Core.Utils.BoolWithReason(true, Tr.Action__Confirm);
                }, "".AsLoc())
                .OnClick(() =>
                {
                    m_tradeInProgress = true;

                    if (contract.Demand.Quantity > tradeDock.Prototype.Capacity - tradeDock.GetQuantity())
                    {
                        context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
                        m_tradeInProgress = false;
                        return;
                    }
                    MultiplayerTradeManager.ClaimContract(tradeDock.Address, tradeDock.Authorization, offer)
                        .ContinueWith((result) =>
                        {
                            if (result.Status == TaskStatus.RanToCompletion && result.Result)
                            {
                                tradeDock.AddProduct(contract.Demand);
                                removeOffer();
                                RemoveFromHierarchy();
                                context.AudioDb.GetSharedAudioUi(UserInterface.Audio.MoneyAction_prefab);
                            }
                            else
                            {
                                m_tradeInProgress = false;
                                context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
                            }
                        });
                })
            );
        }
    }
}