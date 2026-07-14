using System.Reflection;
using Mafi;
using Mafi.Base.Prototypes.Trains;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Mods;
using Mafi.Core.Trains;
using ProgramableNetwork.Data.Modules;
using static Mafi.Unity.Assets.Unity;

namespace ProgramableNetwork;

public class TransportAndLogisticsLimits : ModuleGroup, IModuleGroup {

	public override void RegisterData(ProtoRegistrator registrator) {
		// Swap groups for the storage-percentage modules.  All four *_Get share output p +
		// field s (Storage), so they form one group.  The *_Set split into two groups by field
		// type: the Flow_* use EntityField<Storage> while the Logistics_* use EntityField<IEntity>
		// (Storage OR train station) — keeping them apart avoids a swap carrying a station entity
		// into a Storage-only field.
		var storageGet = registrator.SwapGroupStart(SwapGroups.StorageLimitGet, Category.Connection.Name);
		var flowSet = registrator.SwapGroupStart(SwapGroups.StorageFlowSet, Category.Connection.Name);
		var logisticsSet = registrator.SwapGroupStart(SwapGroups.StorageLogisticsSet, Category.Connection.Name);
		Transport(registrator, storageGet, flowSet);
		Logistics(registrator, storageGet, logisticsSet);
		storageGet.RegisterSwapable();
		flowSet.RegisterSwapable();
		logisticsSet.RegisterSwapable();
	}

	private void Transport(ProtoRegistrator registrator, ModuleSwapGroupBuilder storageGet, ModuleSwapGroupBuilder flowSet) {
		registrator
			.ModuleBuilderStart("Connection_Storage_Flow_In_Set", "Connection: Flow (in, set)", "FS")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(2)
			.AddInput("p", "Percentage")
			.AddEntityField<Storage>("s", "Storage")
			.Action(m => {
				Storage storage = m.Field.Entity<Storage>("s");
				if (storage is null) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
				if (percentage != storage.TransportUntilPercent) {
					storage.SetTransportUntilPercent(percentage);
				}

				return ModuleStatus.Running;
			})
			.AddDisplayFiller(1)
			.AddDisplay("t", "Type", 1, image: true)
			.Display(m => m.Display["t"] = $"#CAAAA00{UserInterface.Toolbar.Transports_svg}")
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(flowSet);

		registrator
			.ModuleBuilderStart("Connection_Storage_Flow_In_Get", "Connection: Flow (in, get)", "FG")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(1)
			.AddOutput("p", "Percentage")
			.AddEntityField<Storage>("s", "Storage")
			.Action(m => {
				Storage storage = m.Field.Entity<Storage>("s");
				if (storage is null) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				m.Output["p"] = storage.TransportUntilPercent.ToIntPercentRounded();
				return ModuleStatus.Running;
			})
			.AddDisplay("p", "Percentage", 1, defaultText: "#CAAAA00100")
			.Display(m => {
				m.Display["p"] = $"#CAAAA00{m.Output["p"].IntegerPart}";
			})
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(storageGet);

		registrator
			.ModuleBuilderStart("Connection_Storage_Flow_Out_Set", "Connection: Flow (out, set)", "FS")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(2)
			.AddInput("p", "Percentage")
			.AddEntityField<Storage>("s", "Storage")
			.Action(m => {
				Storage storage = m.Field.Entity<Storage>("s");
				if (storage is null) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
				if (percentage != storage.TransportFromPercent) {
					storage.SetTransportFromPercent(percentage);
				}

				return ModuleStatus.Running;
			})
			.AddDisplayFiller(1)
			.AddDisplay("t", "Type", 1, image: true)
			.Display(m => m.Display["t"] = $"#C6688FF{UserInterface.Toolbar.Transports_svg}")
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(flowSet);

		registrator
			.ModuleBuilderStart("Connection_Storage_Flow_Out_Get", "Connection: Flow (out, get)", "FG")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(1)
			.AddOutput("p", "Percentage")
			.AddEntityField<Storage>("s", "Storage")
			.Action(m => {
				Storage storage = m.Field.Entity<Storage>("s");
				if (storage is null) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				m.Output["p"] = storage.TransportFromPercent.ToIntPercentRounded();
				return ModuleStatus.Running;
			})
			.AddDisplay("p", "Percentage", 1, defaultText: "#CC6688FF0")
			.Display(m => {
				m.Display["p"] = $"#C6688FF{m.Output["p"].IntegerPart}";
			})
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(storageGet);
	}

	private void Logistics(ProtoRegistrator registrator, ModuleSwapGroupBuilder storageGet, ModuleSwapGroupBuilder logisticsSet) {
		registrator
			.ModuleBuilderStart("Connection_Storage_Logistics_In_Set", "Connection: Logistics (in, set)", "LS")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(2)
			.AddInput("p", "Percentage")
			.AddEntityField<IEntity>("s", "Storage / Station", "Select storage or station to set requested amount",
				filter: (m, e) => e is Storage or ITrainStationRootWithRules)
			.Action(m => {
				if (m.Field.Entity<ILayoutEntity>("s") is not { } entity) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
				if (entity is Storage storage) {
					if (percentage != storage.TransportUntilPercent) {
						storage.SetImportPercent(percentage);
					}
				} else if (entity is TrainStationModule module) {
					if (module.StoredProduct.IsNone) {
						return ModuleStatus.Running;
					}
					typeof(TrainStationModule)
						.GetField("m_stationManager", BindingFlags.Instance | BindingFlags.NonPublic)!
						.GetValue(module)
						.As<TrainStationManager>()
						.TryGetRootEntityForStation(module, out ITrainStationRoot root);
					root.As<ITrainStationRootWithRules>()!
						.SetThresholdFor(
							module.StoredProduct.Value,
							Percent.Hundred - percentage);
				}
				return ModuleStatus.Running;
			})
			.AddDisplayFiller(1)
			.AddDisplay("t", "Type", 1, image: true)
			.Display(m => m.Display["t"] = $"#C00AA00{UserInterface.Toolbar.Vehicles_svg}")
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(logisticsSet);

		registrator
			.ModuleBuilderStart("Connection_Storage_Logistics_In_Get", "Connection: Logistics (in, get)", "LG")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(1)
			.AddOutput("p", "Percentage")
			.AddEntityField<Storage>("s", "Storage")
			.Action(m => {
				Storage storage = m.Field.Entity<Storage>("s");
				if (storage is null) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				m.Output["p"] = storage.ImportUntilPercent.ToIntPercentRounded();
				return ModuleStatus.Running;
			})
			.AddDisplay("p", "Percentage", 1, defaultText: "#C00AA000")
			.Display(m => {
				m.Display["p"] = $"#C00AA00{m.Output["p"].IntegerPart}";
			})
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(storageGet);

		registrator
			.ModuleBuilderStart("Connection_Storage_Logistics_Out_Set", "Connection: Logistics (out, set)", "LS")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(2)
			.AddInput("p", "Percentage")
			.AddEntityField<IEntity>("s", "Storage / Station", "Select storage or station to set requested amount",
				filter: (m, e) => e is Storage or ITrainStationRootWithRules)
			.Action(m => {
				if (m.Field.Entity<ILayoutEntity>("s") is not { } entity) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
				if (entity is Storage storage) {
					if (percentage != storage.TransportUntilPercent) {
						storage.SetExportPercent(percentage);
					}
				} else if (entity is TrainStationModule module) {
					if (module.StoredProduct.IsNone) {
						return ModuleStatus.Running;
					}
					typeof(TrainStationModule)
						.GetField("m_stationManager", BindingFlags.Instance | BindingFlags.NonPublic)!
						.GetValue(module)
						.As<TrainStationManager>()
						.TryGetRootEntityForStation(module, out ITrainStationRoot root);
					root.As<ITrainStationRootWithRules>()!
						.SetThresholdFor(
							module.StoredProduct.Value,
							percentage);
				}
				return ModuleStatus.Running;
			})
			.AddDisplayFiller(1)
			.AddDisplay("t", "Type", 1, image: true)
			.Display(m => m.Display["t"] = $"#CCC0000{UserInterface.Toolbar.Vehicles_svg}")
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(logisticsSet);

		registrator
			.ModuleBuilderStart("Connection_Storage_Logistics_Out_Get", "Connection: Logistics (out, get)", "LG")
			.AddCategory(Category.Connection)
			.AddCategory(Category.ConnectionWrite)
			.AddCategory(Category.Control)
			.Width(1)
			.AddOutput("p", "Percentage")
			.AddEntityField<Storage>("s", "Storage")
			.Action(m => {
				Storage storage = m.Field.Entity<Storage>("s");
				if (storage is null) {
					m.SetError("Storage is not connected");
					return ModuleStatus.Error;
				}

				m.Output["p"] = storage.ExportFromPercent.ToIntPercentRounded();
				return ModuleStatus.Running;
			})
			.AddDisplay("p", "Percentage", 1, defaultText: "#CCC0000100")
			.Display(m => {
				m.Display["p"] = $"#CCC0000{m.Output["p"].IntegerPart}";
			})
			.AddControllerDevice()
			.BuildAndAdd()
			.EnlistSwapable(storageGet);
	}
}
