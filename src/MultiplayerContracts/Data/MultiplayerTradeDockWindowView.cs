using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Contracts;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;

namespace MultiplayerContracts
{
    internal class MultiplayerTradeDockWindowView : EntityInspectorBase<MultiplayerTradeVillage>
    {
        private readonly MultiplayerTradeDockInspector m_controller;
        private readonly ContractsManager m_contractManager;
        private readonly ProtosDb m_protoDb;
        private static int indexer = 0;
        private Txt label;
        private Proto.ID createdId;
        private Btn btnCreate;
        private Btn btnRemove;
        private ContractsManager.EstablishCheckResult establishStatus;
        private ContractsManager.CancelCheckResult cancelStatus;

        public MultiplayerTradeDockWindowView(MultiplayerTradeDockInspector inspector, ContractsManager contractManager, ProtosDb protosDb) : base(inspector)
        {
            this.m_controller = inspector;
            this.m_contractManager = contractManager;
            this.m_protoDb = protosDb;
        }

        protected override MultiplayerTradeVillage Entity => m_controller.SelectedEntity;

        public class Texts
        {
            public static readonly string NotCreated = "Contract not created";
        }

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            base.AddCustomItems(itemContainer);

            btnCreate = new Btn(Builder, "btnCreate");
            btnCreate.SetText("Create contract");
            btnCreate.OnClick(this.CreateContract);
            btnCreate.SetButtonStyle(Style.Global.GeneralBtn);
            itemContainer.Add(10, btnCreate, 18);

            btnRemove = new Btn(Builder, "btnRemove");
            btnRemove.SetText("Remove contract");
            btnRemove.SetButtonStyle(Style.Global.GeneralBtn);
            btnRemove.OnClick(this.RemoveContract);
            btnRemove.SetEnabled(false);

            itemContainer.Add(11, btnRemove, 18);

            label = new Txt(Builder, "lblContract");
            label.SetText(Texts.NotCreated);
            itemContainer.Add(12, label, 18);
        }

        private void CreateContract()
        {
        }

        private void RemoveContract()
        {
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            if (m_controller.SelectedEntity == null)
                return;

            if (establishStatus == ContractsManager.EstablishCheckResult.Ok)
            {
                btnCreate.SetEnabled(false);
                btnRemove.SetEnabled(true);
            }

            if (cancelStatus == ContractsManager.CancelCheckResult.Ok)
            {
                btnCreate.SetEnabled(true);
                btnRemove.SetEnabled(false);
            }
        }
    }
}