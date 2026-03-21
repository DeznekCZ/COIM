using Mafi;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Products;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using static Mafi.Unity.Assets.Unity;

namespace MultiplayerContracts.Data {
	internal class TradeOfferViewCreate : PanelRow {
		private bool m_tradeInProgress;

		private static ProductProto supplyProto;
		private static int supplyCount;

		private static ProductProto demandProto;
		private static int demandCount;

		public TradeOfferViewCreate(MultiplayerTradeDock tradeDock, Action add, IAssetTransactionManager assetsManager, UiContext context)
			: base(gap: 5.px(), noBolts: true) {
			this.ClassRemove(Cls.panel);
			this.ClassRemove(Cls.shadowAll);
			this.Class(Cls.group);

			Body.AddAndReturn(new SingleProductPickerUi(
				allAvailableProducts: () => context.ProtosDb.All<ProductProto>()
															.Where(p => p.IsAvailable && p.IsStorable),
				onProductSelected: (proto) => supplyProto = proto,
				selectedProduct: () => supplyProto.CreateOption(),
				onClear: () => supplyProto = null,
				emptyTooltip: Tr.SellPrefix
			));

			Body.AddAndReturn(new TextField())
				.NumericOnly()
				.Width(80)
				.Height(Percent.Hundred)
				.ObserveEnabled(() => supplyProto != null)
				.ObserveValue(() => supplyCount)
				.OnValueChanged((fix) => supplyCount = int.Parse(fix));

			Body.AddAndReturn(new Column(gap: 5.px())
			{
                // Maximum selectable
                new Label().Height(20).FontBold()
					.With(l => l.Observe(() => tradeDock.Prototype.Capacity).Do(quantity => l.Value($"/{quantity.Value}".AsLoc()))),
                // Display available
                new Label().Height(11)
					.With(l => l.Observe(() => {
						if (supplyProto == null) {
							return Quantity.Zero;
						} else {
							return assetsManager.GetAvailableQuantityForRemoval(supplyProto);
						}
					}).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
			});

			Body.AddAndReturn(new SingleProductPickerUi(
				allAvailableProducts: () => context.ProtosDb.All<ProductProto>()
															.Where(p => p.IsAvailable && p.IsStorable),
				onProductSelected: (proto) => demandProto = proto,
				selectedProduct: () => demandProto.CreateOption(),
				onClear: () => demandProto = null,
				emptyTooltip: Tr.BuyPrefix
			));

			Body.AddAndReturn(new TextField())
				.NumericOnly()
				.Width(80)
				.Height(Percent.Hundred)
				.ObserveEnabled(() => demandProto != null)
				.ObserveValue(() => demandCount)
				.OnValueChanged((fix) => demandCount = int.Parse(fix));

			Body.AddAndReturn(new Column(gap: 5.px())
			{
                // Maximum selectable
                new Label().Height(20).FontBold()
					.With(l => l.Observe(() => tradeDock.Prototype.Capacity).Do(quantity => l.Value($"/{quantity.Value}".AsLoc()))),
                // Display available
                new Label().Height(11)
					.With(l => l.Observe(() => {
						if (demandProto == null) {
							return Quantity.Zero;
						} else {
							return assetsManager.GetAvailableQuantityForRemoval(demandProto);
						}
					}).Do(quantity => l.Value($"({quantity.Value})".AsLoc())))
			});

			Body.AddAndReturn(new ButtonIcon(Button.Unity, UserInterface.Toolbar.TradeClipped_svg)
				.NoShrink()
				.MarginLeft(2.pt())
				.ObserveEnabledWithReason(() => {
					if (supplyCount > tradeDock.Capacity.Value) {
						return new Mafi.Core.Utils.BoolWithReason(
							false, MpTr.QuantityAssertion_OfferAboveCapacity.Format(tradeDock.Capacity.Value.ToLocCached()));
					}

					if (demandCount > tradeDock.Prototype.Capacity.Value) {
						return new Mafi.Core.Utils.BoolWithReason(
							false, MpTr.QuantityAssertion_DemandAboveCapacity.Format(tradeDock.Capacity.Value.ToLocCached()));
					}

					if (supplyCount == 0) {
						return new Mafi.Core.Utils.BoolWithReason(
							false, MpTr.QuantityAssertion_OfferAboveZero);
					}

					if (demandCount == 0) {
						return new Mafi.Core.Utils.BoolWithReason(
							false, MpTr.QuantityAssertion_OfferAboveZero);
					}

					return new Mafi.Core.Utils.BoolWithReason(true, "".AsLoc());
				}, "".AsLoc())
				.OnClick(() => {
					m_tradeInProgress = true;

					var contract = new ContractParameters(
							new ProductQuantity(supplyProto, supplyCount.Quantity()),
							new ProductQuantity(demandProto, demandCount.Quantity())
						);

					if (contract.Supply.Quantity > tradeDock.Prototype.Capacity - tradeDock.GetQuantity()) {
						context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
						m_tradeInProgress = false;
						return;
					}

					var much = assetsManager.RemoveAsMuchAs(contract.Supply, Mafi.Core.Products.DestroyReason.QuickTrade);
					if (much != contract.Supply.Quantity) {
						tradeDock.AddProduct(much.Of(contract.Supply.Product));
						context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
						return;
					}
					MultiplayerTradeManager.CreateContract(tradeDock.Address, tradeDock.Authorization, contract)
						.ContinueWith((result) => {
							if (result.Result) {
								add();
								context.AudioDb.GetSharedAudioUi(UserInterface.Audio.MoneyAction_prefab);
							} else {
								tradeDock.AddProduct(contract.Supply);
								context.AudioDb.GetSharedAudioUi(UserInterface.Audio.InvalidOp_prefab);
								m_tradeInProgress = false;
							}
						});
				})
			);
		}
	}
}