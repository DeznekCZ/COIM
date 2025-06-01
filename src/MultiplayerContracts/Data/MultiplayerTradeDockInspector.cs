using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using MultiplayerContracts.Data.Entries;
using static Mafi.Unity.Assets.Unity;

namespace MultiplayerContracts.Data
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    internal class MultiplayerTradeDockInspector : BaseInspector<MultiplayerTradeDock>
    {
        private readonly TabContainer m_tabs;
        private readonly Column m_marketList;
        private readonly Column m_offerList;
        private readonly Column m_creator;
        private ContractLists m_list;
        private bool m_newOffers;
        private bool m_newClaims;
        private bool m_isRefeshing;
        private bool m_isNotResponding;
        private Column m_storage;

        public MultiplayerTradeDockInspector(UiContext context) : base(context)
        {
            m_list = new ContractLists();

            AddPanelRow(
                new Label("Market".AsLoc())
                    .Class(Cls.title)
                    .TextAlign(TextAlignment.LeftMiddle)
                    .Height(32.px()), // medium
                new UiComponent().FlexGrow(1),
                new TextField().With(field =>
                {
                    field.Width(200);
                    field.ObserveValue(() => Entity?.Address ?? "localhost:6542");
                    field.OnValueChanged((e) =>
                    {
                        if (Entity != null)
                        {
                            Entity.Market = e;
                        }
                    });
                    field.Height(32.px()); // medium
                }),
                new ButtonIcon(UserInterface.General.Repeat_svg)
                    .Medium()
                    .Height(32.px())
                    .OnClick(RefreshList)
                    .ObserveEnabledWithReason(() =>
                    {
                        if (Entity is null)
                        {
                            return new Mafi.Core.Utils.BoolWithReason(false, "No market connected".AsLoc());
                        }

                        if (m_isRefeshing)
                        {
                            Status.As("Searching for product offers".AsLoc(), DisplayState.Warning);
                            return new Mafi.Core.Utils.BoolWithReason(false, "Searching for product offers".AsLoc());
                        }

                        if (m_isNotResponding)
                        {
                            Status.As("Market does not responding".AsLoc(), DisplayState.Danger);
                            return new Mafi.Core.Utils.BoolWithReason(false, "Market does not responding".AsLoc());
                        }

                        Status.As(Tr.EntityStatus__Working, DisplayState.Positive);
                        return new Mafi.Core.Utils.BoolWithReason(true, "Update list of items".AsLoc());
                    }, "Update list of items".AsLoc())
            );

            m_tabs = new TabContainer().Height(400);
            MainBody.Add(m_tabs);

            this.Observe(() => Entity)
                .Do(dock => RefreshList());

            // Panel for show results
            m_tabs.AddTab("Market offers".AsLoc(), m_marketList = new Column(gap: 5.px()), UserInterface.General.Tradable128_png, "Contains offers from other players".AsLoc(), switchTo: true);
            m_tabs.AddTab("Your offers".AsLoc(), m_offerList = new Column(gap: 5.px()), UserInterface.General.Sailor_svg, "Contains your offers to be claimed".AsLoc(), switchTo: false);
            m_tabs.AddTab("Storage".AsLoc(), m_storage = new Column(gap: 5.px()), UserInterface.EntityIcons.Storage_svg, "Displays current storage level".AsLoc(), switchTo: false);

            // observe offers
            this.Observe(() => m_list.Available)
                .Do(list => {
                    m_marketList.Clear();

                    Grid takeList = new Grid(2, 5.px(), 5.px());
                    takeList.Component.Width(Percent.Hundred);
                    m_marketList.Add(takeList.Component);

                    list.ForEach(offer =>
                        takeList.Add(new TradeOfferViewBuy(Entity, offer, m_list.Entries[offer],
                                                () => m_list.Available.Remove(offer), context.AssetsManager, Context))
                    );
                });

            // observe claimables
            this.Observe(() => m_list.Claimable)
                .Observe(() => m_list.Owned)
                .Do((claimable, waiting) => {
                    m_offerList.Clear();

                    // Add creator
                    m_offerList.Add(new TradeOfferViewCreate(Entity, () => RefreshList(), context.AssetsManager, context));

                    m_offerList.Add(new HorizontalDivider());
                    m_offerList.Add(new Label("Claimable".AsLoc()).Width(Percent.Hundred).TextAlign(TextAlignment.CenterMiddle));
                    m_offerList.Add(new HorizontalDivider());

                    if (claimable.Count > 0)
                    {
                        Grid claimList = new Grid(3, 5.px(), 5.px());
                        claimList.Component.Width(Percent.Hundred);
                        m_offerList.Add(claimList.Component);

                        claimable.ForEach(offer =>
                            claimList.Add(new TradeOfferViewClaim(Entity, offer, m_list.Entries[offer],
                                                     () => m_list.Claimable.Remove(offer), context.AssetsManager, Context))
                        );
                    }

                    m_offerList.Add(new HorizontalDivider());
                    m_offerList.Add(new Label("Revokable".AsLoc()).Width(Percent.Hundred).TextAlign(TextAlignment.CenterMiddle));
                    m_offerList.Add(new HorizontalDivider());

                    if (waiting.Count > 0)
                    {
                        Grid revokeList = new Grid(2, 5.px(), 5.px());
                        revokeList.Component.Width(Percent.Hundred);
                        m_offerList.Add(revokeList.Component);
                        waiting.ForEach(offer =>
                            revokeList.Add(new TradeOfferViewReclaim(Entity, offer, m_list.Entries[offer],
                                                     () => m_list.Owned.Remove(offer), context.AssetsManager, Context))
                        );
                    }
                });

            Grid storage = new Grid(10, 5, 5);
            storage.Component.Width(Percent.Hundred);
            m_storage.Add(storage.Component);
            this.Observe(() => Entity.GetQuantities())
                .Do(quantities =>
                {
                    storage.Clear();

                    foreach (Mafi.Core.ProductQuantity item in quantities)
                    {
                        if (item.Quantity == Quantity.Zero)
                            continue;

                        storage.Add(new Column(gap: 2)
                        {
                            new Icon().Value(item.Product).Large(),
                            new Label().Value(item.Quantity).Width(36.px()).TextAlign(TextAlignment.CenterMiddle)
                        }.Width(36.px()).Height(48.px()));
                    }
                });
        }

        private void RefreshList()
        {
            m_isRefeshing = true;

            MultiplayerTradeManager.Register(Entity.Address, Entity.Authorization, Entity.Id)
                .ContinueWith((newKey) =>
                {
                    if (newKey.IsFaulted || newKey.IsCanceled)
                    {
                        m_isRefeshing = false;
                        return;
                    }

                    Entity.MarketAuthentications[Entity.Market] = newKey.Result;

                    MultiplayerTradeManager.GetContracts(
                                    Entity.Market,
                                    Entity.Authorization)
                        .ContinueWith(list =>
                        {
                            if (list.IsFaulted || list.IsCanceled)
                            {
                                m_list = new ContractLists();
                                m_isRefeshing = false;
                                return;
                            }

                            Log.Debug($"Get contracts status: {list.Status}");
                            m_list = list.Result;

                            // TODO create comparison and display for new entries
                            m_newOffers = false;
                            m_newClaims = false;

                            m_isRefeshing = false;
                        });
                });
        }
    }
}
