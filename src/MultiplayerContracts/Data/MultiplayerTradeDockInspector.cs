using Mafi;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Contracts;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;

namespace MultiplayerContracts
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    internal class MultiplayerTradeDockInspector : EntityInspector<MultiplayerTradeDock, MultiplayerTradeDockWindowView>
    {
        private readonly ContractsManager m_contractManager;
        private readonly MultiplayerTradeDockWindowView m_windowView;
        private readonly ProtosDb m_protosDb;

		public MultiplayerTradeDockInspector(InspectorContext inspectorContext, ContractsManager contractManager, ProtosDb protosDb)
			: base(inspectorContext)
		{
			m_contractManager = contractManager;
			m_protosDb = protosDb;
			m_windowView = new MultiplayerTradeDockWindowView(this, m_contractManager, m_protosDb);
		}

		protected override void OnActivated()
		{
			base.OnActivated();
		}

		protected override MultiplayerTradeDockWindowView GetView()
		{
			return m_windowView;
		}
	}
}
