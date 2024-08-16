using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Syncers;
using Mafi.Core.World.Contracts;
using Mafi.Core.World.Loans;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Threading.Tasks;

namespace MultiplayerContracts
{
    internal class MultiplayerTradeDockWindowView : StaticEntityInspectorBase<MultiplayerTradeDock>
    {
        private readonly MultiplayerTradeDockInspector m_controller;
        private readonly ContractsManager m_contractManager;
        private readonly ProtosDb m_protoDb;
        private static int indexer = 0;
        private bool m_isContactDefinitionValid;
        private Txt label;
        private Proto.ID createdId;
        private SelectProduct m_demandProduct;
        private TxtField m_demandField;
        private SelectProduct m_supplyProduct;
        private TxtField m_supplyField;
        private ContractsManager.EstablishCheckResult establishStatus;
        private ContractsManager.CancelCheckResult cancelStatus;
        private int m_supplyInt;
        private int m_demandInt;
        private ContractLists m_list;
        private bool m_tradeActive;
        private bool m_marketOpen;

        private event Action m_wasUpdate;

        public MultiplayerTradeDockWindowView(MultiplayerTradeDockInspector inspector, ContractsManager contractManager, ProtosDb protosDb) : base(inspector)
        {
            this.m_controller = inspector;
            this.m_contractManager = contractManager;
            this.m_protoDb = protosDb;
            this.m_list = new ContractLists();
        }

        protected override MultiplayerTradeDock Entity => m_controller.SelectedEntity;

        public class Texts
        {
            public static readonly string NotCreated = "Contract not created";
        }

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            MakeScrollableWithHeightLimit();

            base.AddCustomItems(itemContainer);

            itemContainer.SetItemSpacing(5);

            var tabs = new MyTabContainer(Builder, Style, "tabs", () => { })
                .SetTabCellSize(new UnityEngine.Vector2(130, 40))
                .AppendTo(itemContainer);

            var allMarketHolder = Builder
                .NewStackContainer("market_tab")
                .SetWidth(515)
                .SetItemSpacing(5)
                .SetInnerPadding(Offset.All(5))
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .AppendTo(tabs, "Markets", out var marketsTab);

            var marketSelection = Builder
                .NewStackContainer("market_entry")
                .SetWidth(505)
                .SetItemSpacing(5)
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .AppendTo(allMarketHolder);

            var marketAddress = Builder
                .NewTxtField("market_address")
                .SetPlaceholderText("Market address")
                .SetWidth(195)
                .AppendTo(marketSelection);

            var marketName = Builder
                .NewTxt("market_address")
                .SetText("Market")
                .SetWidth(195)
                .AppendTo(marketSelection);

            var joinMarket = Builder
                .NewBtnGeneral("market_join")
                .SetText("Join market")
                .SetWidth(100)
                .OnClick(() =>
                {
                    Entity.Market = marketAddress.GetText();
                })
                .AppendTo(marketSelection);

            var refreshButton = Builder
                .NewBtnGeneral("refresh");
            refreshButton
                .SetSize(100, 40)
                .SetText("Get offers")
                .OnClick(RefreshList)
                .AppendTo(itemContainer);
            m_wasUpdate += () => refreshButton.SetEnabled(!m_tradeActive && m_marketOpen);

            var marketHolder = Builder
                .NewStackContainer("market_holder")
                .SetWidth(515)
                .SetItemSpacing(5)
                .SetInnerPadding(Offset.All(5))
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .AppendTo(allMarketHolder);

            var allMyOffersContainer = Builder
                .NewStackContainer("new_offer_tab")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetWidth(515)
                .SetInnerPadding(Offset.All(5))
                .AppendTo(tabs, "My offers (0)", out var myOfferTab);

            var newOfferContainer = Builder
                .NewStackContainer("new_offer_info")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetHeight(40)
                .SetWidth(515)
                .SetItemSpacing(5)
                .SetInnerPadding(Offset.All(5))
                .SetBackground(ColorRgba.DarkDarkGray)
                .AppendTo(allMyOffersContainer);

            allMyOffersContainer.AppendDivider(5, ColorRgba.DarkDarkGray);

            m_supplyProduct = new SelectProduct("suply_product", Builder, ValidateContractDefinition, this, m_controller.Context)
                .AppendTo(newOfferContainer);

            m_supplyField = Builder
                .NewTxtField("suply_edit")
                .SetWidth(50)
                .SetHeight(40)
                .SetPlaceholderText("supply")
                .SetOnValueChangedAction(ValidateContractDefinition)
                .AppendTo(newOfferContainer);

            var suplyTxt = Builder
                .NewTxt("suply_stock")
                .SetWidth(50)
                .SetHeight(40)
                .AppendTo(newOfferContainer);

            Builder.NewIconContainer("arrow")
                .SetIcon(Builder.Style.Icons.Transform, Builder.Style.Global.UpointsBtn.BackgroundClr.Value)
                .SetWidth(40)
                .AppendTo(newOfferContainer);

            m_demandProduct = new SelectProduct("demand_product", Builder, ValidateContractDefinition, this, m_controller.Context)
                .AppendTo(newOfferContainer);

            m_demandField = Builder
                .NewTxtField("demand_edit")
                .SetWidth(50)
                .SetHeight(40)
                .SetPlaceholderText("demand")
                .SetOnValueChangedAction(ValidateContractDefinition)
                .AppendTo(newOfferContainer);

            var demandTxt = Builder
                .NewTxt("demand_stock")
                .SetWidth(50)
                .SetHeight(40)
                .AppendTo(newOfferContainer);

            var btnCreate = Builder
                .NewBtnGeneral("btnCreate")
                .SetText("Create")
                .SetWidth(50)
                .SetHeight(40)
                .OnClick(this.CreateContract)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .AppendTo(newOfferContainer);

            var myOfferHolder = Builder
                .NewStackContainer("my_offer_holder")
                .SetWidth(515)
                .SetItemSpacing(5)
                .SetInnerPadding(Offset.All(5))
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .AppendTo(allMyOffersContainer);

            var otherHolder = Builder
                .NewStackContainer("other_holder")
                .SetWidth(515)
                .SetItemSpacing(5)
                .SetInnerPadding(Offset.All(5))
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .AppendTo(tabs, "Market (0)", out var otherTab);

            var storageHolder = Builder
                .NewGridContainer("storage_holder")
                .SetWidth(515)
                .SetCellSpacing(5)
                .SetCellSize(new UnityEngine.Vector2(40, 60))
                .SetDynamicHeightMode(515 / 45)
                .SetInnerPadding(Offset.All(5))
                .SetHeight(260)
                .AppendTo(tabs, "Storage (0)", out var storageTab);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();
            updaterBuilder.Observe(() =>
                    m_isContactDefinitionValid &&
                    m_marketOpen &&
                    IsEnoughSupply())
                .Do(enabled => btnCreate.SetEnabled(enabled));

            updaterBuilder.Observe(() => GetAmount(m_supplyProduct))
                .Do(count => suplyTxt.SetText($"({count})"));

            updaterBuilder.Observe(() => GetAmount(m_demandProduct))
                .Do(count => demandTxt.SetText($"({count})"));

            updaterBuilder.Observe(() => DateTime.Now.Ticks)
                .Do(updated => m_wasUpdate?.Invoke());

            updaterBuilder.Observe(() => m_list.UpdatedAt.Ticks)
                .Do(updated =>
                {
                    myOfferHolder.ClearAndDestroyAll();
                    otherHolder.ClearAndDestroyAll();

                    Log.Debug("List changed");
                    HandleMyOffers(tabs, myOfferTab, myOfferHolder);
                    HandleOtherOffers(tabs, otherTab, otherHolder);
                });

            updaterBuilder.Observe(() => m_tradeActive)
                .Do(updated => m_wasUpdate?.Invoke());

            updaterBuilder.Observe(() => m_marketOpen)
                .Do(updated => m_wasUpdate?.Invoke());

            updaterBuilder.Observe(() => m_controller.SelectedEntity?.GetQuantity() ?? Quantity.Zero)
                .Do(count =>
                {
                    storageHolder.ClearAllAndDestroy();

                    storageTab.TabButton.SetText($"Storage ({count})");

                    if (count.Value > 0)
                    {
                        var quantities = m_controller.SelectedEntity.GetQuantities();

                        foreach (var quant in quantities)
                        {
                            var stack = Builder
                                .NewStackContainer(quant.Product.Id.Value + "_stack")
                                .SetSize(40, 60)
                                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                                .AppendTo(storageHolder);

                            Builder
                                .NewIconContainer(quant.Product.Id.Value + "_icon")
                                .SetSize(40, 40)
                                .SetIcon(quant.Product.IconPath)
                                .AppendTo(stack);

                            Builder
                                .NewTxt(quant.Product.Id.Value + "_txt")
                                .SetSize(40, 40)
                                .SetAlignment(UnityEngine.TextAnchor.MiddleLeft)
                                .SetText(quant.Quantity.Value.ToString())
                                .AppendTo(stack);
                        }
                    }
                });

            updaterBuilder.Observe(() => m_controller.SelectedEntity?.Market)
                .Do(market => marketAddress.SetText(market));

            updaterBuilder.Observe(() => m_controller.SelectedEntity?.MarketName)
                .Do(market => marketName.SetText(market));

            m_marketChanged = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity?.Market);
            m_authorizationChanged = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity?.Authorization);

            AddUpdater(updaterBuilder.Build());

            ExportPriorityPanel customPriorityPanel = new ExportPriorityPanel(this, m_controller.InputScheduler, Builder, () => Entity, "CargoExportPrio");
            customPriorityPanel.PutToRightMiddleOf(this, customPriorityPanel.GetSize(), Offset.Right(0f - customPriorityPanel.GetWidth() + 1f));
            AddUpdater(customPriorityPanel.Updater);

            SetWidth(525);
        }

        private void RefreshList()
        {
            m_tradeActive = true;

            MultiplayerTradeManager.GetContracts(
                            m_controller.SelectedEntity.Market,
                            m_controller.SelectedEntity.Authorization)
                .ContinueWith(list =>
                {
                    Log.Debug($"Get contracts status: {list.Status}");
                    m_list = list.Result;
                    m_tradeActive = false;
                });
        }

        private void CreateView(StackContainer holder, long contractId, ContractParameters contractParameters,
            string buttonText, Func<long, ContractParameters, Task<bool>> buttonAction, Func<ContractParameters, bool> buttonActive)
        {
            var newOfferContainer = Builder
               .NewStackContainer("new_offer_info")
               .SetStackingDirection(StackContainer.Direction.LeftToRight)
               .SetHeight(40)
               .SetWidth(515)
               .SetItemSpacing(5)
               .SetInnerPadding(Offset.All(5))
               .SetBackground(ColorRgba.DarkDarkGray)
               .AppendTo(holder);

            var supplyProduct = Builder
                .NewIconContainer("suply_product")
                .SetIcon(contractParameters.Supply.Product.IconPath)
                .SetSize(40, 40)
                .AppendTo(newOfferContainer);

            var supplyField = Builder
                .NewTxt("suply_edit")
                .SetWidth(50)
                .SetHeight(40)
                .SetText(contractParameters.Supply.Quantity.Value.ToString())
                .SetAlignment(UnityEngine.TextAnchor.MiddleLeft)
                .AppendTo(newOfferContainer);

            var suplyTxt = Builder
                .NewTxt("suply_stock")
                .SetWidth(50)
                .SetHeight(40)
                .SetAlignment(UnityEngine.TextAnchor.MiddleLeft)
                .AppendTo(newOfferContainer);

            Builder.NewIconContainer("arrow")
                .SetIcon(Builder.Style.Icons.Transform, Builder.Style.Global.UpointsBtn.BackgroundClr.Value)
                .SetWidth(40)
                .AppendTo(newOfferContainer);

            var demandProduct = Builder
                .NewIconContainer("demand_product")
                .SetIcon(contractParameters.Demand.Product.IconPath)
                .SetSize(40, 40).AppendTo(newOfferContainer);

            var demandField = Builder
                .NewTxt("demand_edit")
                .SetWidth(50)
                .SetHeight(40)
                .SetText(contractParameters.Demand.Quantity.Value.ToString())
                .SetAlignment(UnityEngine.TextAnchor.MiddleLeft)
                .AppendTo(newOfferContainer);

            var demandTxt = Builder
                .NewTxt("demand_stock")
                .SetWidth(50)
                .SetHeight(40)
                .SetAlignment(UnityEngine.TextAnchor.MiddleLeft)
                .AppendTo(newOfferContainer);

            var btnCreate = Builder
                .NewBtnGeneral("btnCreate");
            btnCreate
                .SetText(buttonText)
                .SetWidth(50)
                .SetHeight(40)
                .OnClick(() =>
                {
                    m_tradeActive = true;

                    buttonAction(contractId, contractParameters)
                        .ContinueWith(result =>
                        {
                            if (result.Result)
                            {
                                m_wasUpdate -= UpdateTexts;
                                holder.SetItemVisibility(newOfferContainer, false);
                                RefreshList();
                            }
                            else
                            {
                                m_tradeActive = false;
                            }
                        });

                })
                .SetButtonStyle(Style.Global.GeneralBtn)
                .AppendTo(newOfferContainer);

            m_wasUpdate += UpdateTexts;

            void UpdateTexts()
            {
                try
                {
                    if (!newOfferContainer.IsVisible())
                    {
                        m_wasUpdate -= UpdateTexts;
                    }
                    else
                    {
                        btnCreate.SetEnabled(buttonActive(contractParameters) && !m_tradeActive && m_marketOpen);
                        suplyTxt.SetText($"({GetAmount(contractParameters.Supply.Product)})");
                        demandTxt.SetText($"({GetAmount(contractParameters.Demand.Product)})");
                    }
                }
                catch (Exception)
                {
                    m_wasUpdate -= UpdateTexts;
                }
            }
        }

        private void HandleMyOffers(MyTabContainer itemContainer, Tab tab, StackContainer holder)
        {
            tab.TabButton.SetText($"My offers ({m_list.Claimable.Count + m_list.Owned.Count})");
            foreach (var owned in m_list.Claimable)
            {
                CreateView(holder, owned, m_list.Entries[owned], "Claim", ClaimItem, (cp) => true);
            }
            holder.AppendDivider(5, ColorRgba.DarkDarkGray);
            foreach (var owned in m_list.Owned)
            {
                CreateView(holder, owned, m_list.Entries[owned], "Remove", RemoveItem, (cp) => true);
            }
        }

        private Task<bool> ClaimItem(long contractId, ContractParameters contractParameters)
        {
            var shop = m_controller.SelectedEntity;
            return MultiplayerTradeManager.ClaimContract(
                            m_controller.SelectedEntity.Market,
                            m_controller.SelectedEntity.Authorization,
                            contractId)
                .ContinueWith(task =>
                {
                    if (task.Result)
                    {
                        shop.AddProduct(contractParameters.Demand);
                        RefreshList();
                        return true;
                    }
                    return false;
                });
        }

        private Task<bool> RemoveItem(long contractId, ContractParameters contractParameters)
        {
            var shop = m_controller.SelectedEntity;
            return MultiplayerTradeManager.RemoveContract(
                            m_controller.SelectedEntity.Market,
                            m_controller.SelectedEntity.Authorization,
                            contractId)
                .ContinueWith(task =>
                {
                    if (task.Result)
                    {
                        shop.AddProduct(contractParameters.Supply);
                        RefreshList();
                    }
                    return task.Result;
                });
        }

        private void HandleOtherOffers(MyTabContainer itemContainer, Tab tab, StackContainer holder)
        {
            tab.TabButton.SetText($"Market ({m_list.Available.Count})");
            foreach (var other in m_list.Available)
            {
                CreateView(holder, other, m_list.Entries[other], "Take", TakeItem,
                    (cp) => GetAmount(cp.Demand.Product) >= cp.Demand.Quantity.Value);
            }
        }

        private Task<bool> TakeItem(long contractId, ContractParameters contractParameters)
        {
            var shop = m_controller.SelectedEntity;

            var assetManager = shop.Context.ProductsManager.AssetManager;
            if (assetManager.TryRemoveProduct(contractParameters.Demand, DestroyReason.Export))
            {
                return MultiplayerTradeManager.TakeContract(
                            m_controller.SelectedEntity.Market,
                            m_controller.SelectedEntity.Authorization,
                            contractId)
                    .ContinueWith(finished =>
                    {
                        if (finished.Result)
                        {
                            shop.AddProduct(contractParameters.Supply);
                            return true;
                        }
                        else
                        {
                            shop.AddProduct(contractParameters.Demand);
                            return false;
                        }
                    });
            }
            else
            {
                // Show notififcation, low product
                return Task.FromResult(false);
            }
        }

        private long GetAmount(SelectProduct m_product)
        {
            if (m_product.Product == null)
                return 0;

            return GetAmount(m_product.Product);
        }

        private long GetAmount(ProductProto m_product)
        {
            var stats = m_controller.SelectedEntity
                .Context.ProductsManager.AssetManager
                .GetAvailableQuantityForRemoval(m_product);

            return stats.Value;
        }

        private bool IsEnoughSupply()
        {
            if (m_controller.SelectedEntity == null)
                return false;
            if (m_supplyProduct.Product == null)
                return false;
            if (m_demandInt <= 0)
                return false;

            var stats = m_controller.SelectedEntity
                .Context.ProductsManager.AssetManager
                .GetAvailableQuantityForRemoval(m_supplyProduct.Product);

            if (stats.Value < m_supplyInt)
                return false;

            return true;
        }

        private void ValidateContractDefinition()
        {
            m_isContactDefinitionValid = false;
            if (!ValidNumber(m_supplyField, ref m_supplyInt))
                return;

            if (!ValidNumber(m_demandField, ref m_demandInt))
                return;

            if (m_supplyProduct.Product == null || m_demandProduct.Product == null
                || m_supplyProduct.Product.Id == m_demandProduct.Product.Id)
                return;

            m_isContactDefinitionValid = true;
        }

        private bool ValidNumber(TxtField m_field, ref int m_value)
        {
            if (int.TryParse(m_field.GetText(), out int m_new))
            {
                m_value = m_new;
                return true;
            }
            else
            {
                m_field.SetText(m_value.ToString());
                return false;
            }
        }

        private void CreateContract()
        {
            if (IsEnoughSupply())
            {

                var assetManager = m_controller.SelectedEntity.Context
                    .ProductsManager.AssetManager;
                if (assetManager.TryRemoveProduct(new ProductQuantity(
                    m_supplyProduct.Product, m_supplyInt.Quantity()), DestroyReason.Export)) {
                    
                    m_tradeActive = true;
                    MultiplayerTradeManager
                        .CreateContract(
                            m_controller.SelectedEntity.Market,
                            m_controller.SelectedEntity.Authorization,
                            new ContractParameters(
                                new ProductQuantity(m_supplyProduct.Product, m_supplyInt.Quantity()),
                                new ProductQuantity(m_demandProduct.Product, m_demandInt.Quantity())
                            )
                        ).ContinueWith(id =>
                        {
                            if (id.Result)
                            {
                                RefreshList();
                            }
                        });
                }
                else
                {
                    // Show notififcation, low product
                }
            }
        }

        private int m_connectionTry = 0;
        private ISyncer<string> m_marketChanged;
        private ISyncer<string> m_authorizationChanged;

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            m_connectionTry++;
            if (m_connectionTry >= 60 && m_controller.SelectedEntity != null)
            {
                var entity = m_controller.SelectedEntity;
                var market = entity.Market;

                m_connectionTry = int.MinValue;
                MultiplayerTradeManager.Ping(market)
                    .ContinueWith((t) =>
                    {
                        m_marketOpen = t.Result.Item1;
                        if (m_marketOpen)
                            Log.Debug("Market is open");
                        else
                            Log.Debug("Market not connected");

                        m_connectionTry = 0;
                    });
            }

            if (m_marketChanged.HasChanged && m_marketChanged.GetValueAndReset() != null)
            {
                var entity = m_controller.SelectedEntity;
                var market = entity.Market;

                MultiplayerTradeManager.Register(
                        market,
                        entity.MarketAuthentications.Get(market)
                            .ValueOr($@"{{""EntityId"":{entity.Id.Value}, ""CreationTime"":0}}"),
                        entity.Id
                    )
                    .ContinueWith(t2 =>
                    {
                        if (t2.IsCompleted)
                            entity.MarketAuthentications[market] = t2.Result;
                        else
                            Log.Debug("Registration failed");
                    });
            }

            if (m_authorizationChanged.HasChanged && m_authorizationChanged.GetValueAndReset() != null)
            {
                RefreshList();
            }
        }
    }
}