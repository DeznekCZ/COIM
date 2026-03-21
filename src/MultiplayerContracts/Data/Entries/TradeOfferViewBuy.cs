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
using Mafi.Collections;
using static Mafi.Unity.Assets.Unity;

namespace MultiplayerContracts.Data.Entries {
	internal class TradeOfferViewBuy : Row {
		private bool m_tradeInProgress;
		private readonly Lyst<long> m_trades;
		private readonly ContractParameters m_firstContract;

		public TradeOfferViewBuy(MultiplayerTradeDock tradeDock, Lyst<long> offers, Func<long, ContractParameters> contract, Action<long> removeOffer, IAssetTransactionManager assetsManager, UiContext context)
			: base(gap: 5.px()) {
			m_trades = offers;
			m_firstContract = contract(offers.First);

			this.Class(Cls.group);
			this.Padding(10);
			this.JustifyItemsSpaceBetween();

			this.AddAndReturn(new Icon())
				.Large()
				.Observe((icon) => (icon, m_firstContract.Demand.Product))
				.Do(entry => entry.icon.Value(entry.Product));

			this.AddAndReturn(new Column(gap: 5.px())
			{
				new Label().Height(20).Width(36).FontBold()
					.With(l => l.Observe(() => m_firstContract.Demand.Quantity).Do(quantity => l.Value(quantity.Value.ToFix32()))),
				new Label().Height(11).Width(36)
					.With(l => l.Observe(() => assetsManager.GetAvailableQuantityForRemoval(m_firstContract.Demand.Product)).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
			}.FlexGrow(1));

			this.AddAndReturn(new Icon(UserInterface.General.ArrowRight_svg)
							.Width(24.px())
							.Height(24.px())
						);

			this.AddAndReturn(new Icon())
				.Large()
				.Observe((icon) => (icon, m_firstContract.Supply.Product))
				.Do(entry => entry.icon.Value(entry.Product));

			this.AddAndReturn(new Column(gap: 5.px())
			{
				new Label().Height(20).FontBold()
					.With(l => l.Observe(() => m_firstContract.Supply.Quantity).Do(quantity => l.Value(quantity.Value.ToFix32()))),
				new Label().Height(11)
					.With(l => l.Observe(() => assetsManager.GetAvailableQuantityForRemoval(m_firstContract.Supply.Product)).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
			}.FlexGrow(1));

			this.AddAndReturn(new ButtonIcon(Button.Unity, UserInterface.Toolbar.TradeClipped_svg)
				.NoShrink()
				.MarginLeft(2.pt())
				.ObserveEnabledWithReason(() => {
					if (m_tradeInProgress) {
						return new Mafi.Core.Utils.BoolWithReason(false, MpTr.TradingProgress);
					}

					Quantity quantity = tradeDock.GetQuantity();
					if (m_firstContract.Demand.Quantity > tradeDock.Prototype.Capacity - quantity) {
						return new Mafi.Core.Utils.BoolWithReason(
							false, $"{Tr.EntityStatus__FullStorage}: ({quantity}/{tradeDock.Prototype.Capacity})".AsLoc());
					}

					bool enough = assetsManager.GetAvailableQuantityForRemoval(m_firstContract.Demand.Product) >= m_firstContract.Demand.Quantity;
					return new Mafi.Core.Utils.BoolWithReason(enough,
						enough
							? MpTr.AvailableOffersOfType.Format(offers.Count)
							: Tr.TradeStatus__CantAfford.AsFormatted
					);
				}, MpTr.AvailableOffersOfType.Format(offers.Count))
				.OnClick(() => {
					m_tradeInProgress = true;
					var much = assetsManager.RemoveAsMuchAs(m_firstContract.Demand, Mafi.Core.Products.DestroyReason.QuickTrade);
					if (much != m_firstContract.Demand.Quantity) {
						tradeDock.AddProduct(much.Of(m_firstContract.Demand.Product));
						context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
						return;
					}

					MultiplayerTradeManager.TakeContract(tradeDock.Address, tradeDock.Authorization, m_trades.First)
						.ContinueWith((result) => {
							if (result.Status == TaskStatus.RanToCompletion && result.Result) {
								tradeDock.AddProduct(m_firstContract.Supply);
								removeOffer(m_trades.First);
								RemoveFromHierarchy();
								context.AudioDb.GetSharedAudioUi(UserInterface.Audio.MoneyAction_prefab);
							} else {
								tradeDock.AddProduct(m_firstContract.Demand);
								context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
								m_tradeInProgress = false;
							}
						});
				})
			);
		}
	}
}