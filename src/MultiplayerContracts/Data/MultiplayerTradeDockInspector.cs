using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MultiplayerTradeDockInspector(UiContext context) : base(context)
        {
            m_list = new ContractLists();

            AddPanelWithHeader(
                m_tabs = new TabContainer()
            ).Header.Add(
                new Label("Market".AsLoc()),
                new ButtonIcon(UserInterface.General.Repeat_svg)
                    .Medium()
                    .OnClick(RefreshList)
                    .ObserveEnabledWithReason(() =>
                    {
                        if (Entity is null || Entity.Market.IsNullOrEmpty() || Entity.Authorization.IsNullOrEmpty())
                            return new Mafi.Core.Utils.BoolWithReason(false, "No market connected".AsLoc());

                        if (m_isRefeshing)
                            return new Mafi.Core.Utils.BoolWithReason(false, "Searching for product offers".AsLoc());

                        if (m_isNotResponding)
                            return new Mafi.Core.Utils.BoolWithReason(false, "Market does not responding".AsLoc());

                        return new Mafi.Core.Utils.BoolWithReason(true, "Update list of items".AsLoc());
                    }, "Update list of items".AsLoc())
            );

            this.Observe(() => Entity)
                .Do(dock => RefreshList());


            // Panel for show results
            m_tabs.AddTab("Market offers".AsLoc(), m_marketList = new Column(gap: 5.px()), UserInterface.EntityIcons.Storage_svg, "Contains offers from other players".AsLoc(), switchTo: true);
            m_tabs.AddTab("Your offers".AsLoc(), m_offerList = new Column(gap: 5.px()), UserInterface.EntityIcons.Storage_svg, "Contains your offers to be claimed".AsLoc(), switchTo: false);
            m_tabs.AddTab("New offer".AsLoc(), m_creator = new Column(gap: 5.px()), UserInterface.EntityIcons.Storage_svg, "Contains your offers to be claimed".AsLoc(), switchTo: false);

            // observe offers
            this.Observe(() => m_list.Available)
                .Do(list => {
                    m_marketList.Clear();
                    list.ForEach(offer =>
                        m_marketList.Add(new TradeOfferView(offer, m_list.Entries[offer], () => m_list.Available.Remove(offer), TradeType.Market, context.AssetsManager))
                    );
                });
        }

        private void RefreshList()
        {
            m_isRefeshing = true;

            MultiplayerTradeManager.GetContracts(
                            Entity.Market,
                            Entity.Authorization)
                .ContinueWith(list =>
                {
                    Log.Debug($"Get contracts status: {list.Status}");
                    m_list = list.Result;

                    // TODO create comparison and display for new entries
                    m_newOffers = false;
                    m_newClaims = false;

                    m_isRefeshing = false;
                });
        }
    }
}
