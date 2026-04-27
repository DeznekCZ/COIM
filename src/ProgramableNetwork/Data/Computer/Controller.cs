using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Ports.Io;
using System;
using Mafi.Serialization;
using System.Collections.Generic;
using Mafi.Core.Population;
using Mafi.Core.Prototypes;
using Mafi.Base;
using Mafi.Core.Factory.ElectricPower;
using System.Linq;
using Mafi.Collections;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Maintenance;
using Mafi.Core.Products;
using Mafi.Core.Entities.Static;
using Mafi.Core.Notifications;
using ProgramableNetwork.Data.Mod;
using System.Reflection;
using Mafi.Localization;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Research;

namespace ProgramableNetwork
{
	[GenerateSerializer(false, null, 0)]
	public class Controller : LayoutEntityBase, IAreaSelectableEntity, IEntityWithCloneableConfig, IEntityWithSimUpdate,
		IUnityConsumingEntity, IComputingConsumingEntity, IElectricityConsumingEntity, IMaintainedEntity, IObjectWithCustomTitle
	{
		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate(object obj, BlobWriter writer)
		{
			((Controller) obj).SerializeData(writer);
		};
		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
		{
			((Controller) obj).DeserializeData(reader);
		};

		public Option<string> CustomTitle { get; set; }

		public Controller(EntityId id, ControllerProto proto, TileTransform transform, EntityContext context,
			IEntityMaintenanceProvidersFactory maintenanceProvidersFactory, ResearchManager researchManager)
			: base(id, proto, transform, context)
		{
			Prototype = proto.BasedOn ?? proto;
			ErrorMessage = "";
			Color = proto.DefaultColor;
			m_unityConsumer = Context.UnityConsumerFactory.CreateConsumer(this);
			m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_computingConsumer = Context.ComputingConsumerFactory.CreateConsumer(this);
			m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);
			m_notificationInfoManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.InfoNotification);
			m_notificationWarningManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.WarningNotification);
			m_notificationErrorManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.ErrorNotification);
			ResearchManager = researchManager;
			Modules = new Lyst<Module>();
			Rows = new Lyst<Lyst<ModulePlacement>>();
			for (int i = 0; i < Prototype.Rows; i++)
			{
				Lyst<ModulePlacement> row = new Lyst<ModulePlacement>();
				for (int j = 0; j < Prototype.Columns; j++)
				{
					row.Add((ModulePlacement)(0, true));
				}
				Rows.Add(row);
			}

			Action initSettings = proto.InitModules(this);
			foreach (Module module in Modules)
			{
				module.Prototype.ExecuteInit(module);
			}
			initSettings();

			Log.Info($"Created with {Prototype.Rows} rows, {Prototype.Columns} columns");
		}

		[DoNotSave(0, null)]
		private ControllerProto m_proto;
		[DoNotSave(0, null)]
		private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;

		[DoNotSave(0, null)]
		public new ControllerProto Prototype
		{
			get
			{
				return m_proto;
			}
			protected set
			{
				m_proto = value;
				m_protoId = m_proto.Id;
				base.Prototype = value;
			}
		}

		[DoNotSave(0, null)]
		public override bool CanBePaused => true;
		[DoNotSave(0, null)]
		public bool CanWorkOvertime => true;

		public void AddToConfig(EntityConfigData data)
		{
			// TODO copy modules, name, entity connections, ...
			data.Set<StaticEntityProto.ID>("controller_proto", m_protoId, (str, blob) => blob.WriteString(str.Value));
			data.SetArray<Module>("controller_modules", Modules.ToImmutableArray(), Module.Serialize);
			data.SetArray<Lyst<ModulePlacement>>("controller_rows", Rows.ToImmutableArray(), Lyst<ModulePlacement>.Serialize);
			data.SetInt("controller_speed", Speed);
			data.SetInt("color", (int)Color.Rgba);
		}

		public void ApplyConfig(EntityConfigData data)
		{
			// TODO copy modules, name, entity connections ...
			// TODO validate connection and rotate connection when possible
			var newProto = data.Get("controller_proto", (blob) => new StaticEntityProto.ID(blob.ReadString()));
			if (newProto != m_protoId) {
				// TODO play sound
				return;
			}
			
			var newModules = data.GetArray("controller_modules", Module.Deserialize);
			if (newModules != null)
			{
				this.Modules.Clear();
				this.Modules.AddRange(newModules?.AsEnumerable());
				foreach (Module module in this.Modules)
				{
					module.Context = Context;
					module.Controller = this;
					module.initContexts(-1);
					foreach (IField field in module.Prototype.Fields)
					{
						field.Validate(module);
					}
				}
			}
			var newLocation = data.GetArray("controller_rows", Lyst<ModulePlacement>.Deserialize);
			if (newLocation != null)
			{
				this.Rows.Clear();
				this.Rows.AddRange(newLocation?.AsEnumerable());
			}
			var newSpeed = data.GetInt("controller_speed");
			if (newSpeed != null)
			{
				this.Speed = newSpeed.Value;
			}

			int? color = data.GetInt("color");
			if (color != null)
			{
				this.Color = (uint)color.Value;
			}
		}

		public static void Serialize(Controller value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value))
			{
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		public static Controller Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out Controller value, (Func<BlobReader, Type, Controller>)null))
			{
				reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
			}
			return value;
		}

		[InitAfterLoad(InitPriority.Normal)]
		[OnlyForSaveCompatibility(null)]
		private void initContexts(int saveVersion, DependencyResolver resolver)
		{
			Log.Info($"Initialize context after load");

			Prototype = Context.ProtosDb.Get<ControllerProto>(m_protoId).ValueOrThrow("Invalid controller proto: " + m_protoId);
			m_electricConsumer = m_electricConsumer ?? Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_computingConsumer = m_computingConsumer ?? Context.ComputingConsumerFactory.CreateConsumer(this);
			ResearchManager = resolver.Resolve<ResearchManager>();

			m_notificationInfoManager = WithId(ControllerNotification.InfoNotification, m_notificationInfoManager);
			m_notificationWarningManager = WithId(ControllerNotification.WarningNotification, m_notificationWarningManager);
			m_notificationErrorManager = WithId(ControllerNotification.ErrorNotification, m_notificationErrorManager);

			if (Modules == null)
			{
				Modules = new Lyst<Module>();
			}
			else
			{
				foreach (var m in Modules)
				{
					m.Controller = this;
					m.Context = Context;
					m.initContexts(saveVersion);
					foreach (IField field in m.Prototype.Fields)
					{
						field.Validate(m);
					}

					if (m.Prototype == null) {
						Log.Warning($"Module {m.Id} with null prototype found in controller {Id}, skipping it");
					}
				}
			}

			for (int i = 0; i < Prototype.Rows; i++)
			{
				if (i == Rows.Count)
				{
					var row = new Lyst<ModulePlacement>();
					for (int j = 0; j < Prototype.Columns; j++)
					{
						row.Add((ModulePlacement)(0, true));
					}
					Rows.Add(row);
				}
				else
				{
					var row = Rows[i];
					for (int j = row.Count; j < Prototype.Columns; j++)
					{
						row.Add((ModulePlacement)(0, true));
					}
				}
			}

			if (Color == ColorRgba.Empty)
			{
				Color = m_proto.DefaultColor;
			}

			m_productMaintenanceT1 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT1);
			m_productMaintenanceT2 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT2);
			m_productMaintenanceT3 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT3);
		}

		private EntityNotificator WithId(EntityNotificationProto.ID newNotification, EntityNotificator notification)
		{
			if (!m_reninitNotification)
			{
				return Context.NotificationsManager.CreateNotificatorFor(newNotification);
			}

			PropertyInfo field = typeof(EntityNotificator).GetProperty("NotificationId");
			object v = Context.NotificationsManager.CreateNotificatorFor(newNotification);
			field.SetValue(v, notification.NotificationId);
			return (EntityNotificator)v;
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			writer.WriteString(m_protoId.Value);
			writer.WriteInt(/*Version*/ 3);

			writer.WriteString(ErrorMessage ?? "");
			Option<string>.Serialize(CustomTitle, writer);
			ColorRgba.Serialize(Color, writer);

			writer.WriteInt(GeneralPriority);
			writer.WriteGeneric(m_maintenanceConsumer);
			writer.WriteGeneric(m_electricConsumer);
			writer.WriteGeneric(m_computingConsumer);

			writer.WriteUInt(m_notificationInfoManager.NotificationId.Value);
			writer.WriteUInt(m_notificationWarningManager.NotificationId.Value);
			writer.WriteUInt(m_notificationErrorManager.NotificationId.Value);

			writer.WriteInt(m_clockSpeed);
			writer.WriteInt(m_clock);

			Lyst<Module>.Serialize(Modules, writer);
			Lyst<Lyst<ModulePlacement>>.Serialize(Rows, writer);
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			m_protoId = new Mafi.Core.Entities.Static.StaticEntityProto.ID(reader.ReadString());
			int version = reader.ReadInt();

			CurrentInstruction = 0;

			ErrorMessage = reader.ReadString();
			CustomTitle = Option<string>.Deserialize(reader);
			if (version < 3)
			{
				Color = ColorRgba.Empty;
			}
			else
			{
				Color = ColorRgba.Deserialize(reader);
			}

			GeneralPriority = reader.ReadInt();
			m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();

			if (version >= 1)
			{
				m_electricConsumer = reader.ReadGenericAs<IElectricityConsumer>();
				m_computingConsumer = reader.ReadGenericAs<IComputingConsumer>();

				m_reninitNotification = true;

				object v = m_notificationInfoManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationInfoManager = (EntityNotificator)v;

				v = m_notificationWarningManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationWarningManager = (EntityNotificator)v;

				v = m_notificationErrorManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationErrorManager = (EntityNotificator)v;
			}

			if (version >= 2)
			{
				m_clockSpeed = reader.ReadInt();
				m_clock   = reader.ReadInt();
			}
			else
			{
				m_clockSpeed = 0;
				m_clock   = 0;
			}

			Modules = Lyst<Module>.Deserialize(reader);
			Rows = Lyst<Lyst<ModulePlacement>>.Deserialize(reader);

			Log.Info($"Deserialized with {Modules.Count} modules and {Rows.Count} rows");
			reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
		}

		[DoNotSave(0, null)]
		public Upoints MonthlyUnityConsumed => 0.Upoints();

		[DoNotSave(0, null)]
		public Upoints MaxMonthlyUnityConsumed => 0.Upoints();

		public Proto.ID UpointsCategoryId => IdsCore.UpointsCategories.Boost;

		[DoNotSave(0, null)]
		public Option<UnityConsumer> UnityConsumer => m_unityConsumer;
		[DoNotSave(0, null)]
		private UnityConsumer m_unityConsumer;

		[DoNotSave(0, null)]
		public int CurrentInstruction { get; private set; }

		[DoNotSave(0, null)]
		public Electricity PowerRequired { get; private set; } = Electricity.Zero;

		[DoNotSave(0, null)]
		public Option<IElectricityConsumerReadonly> ElectricityConsumer => ((IElectricityConsumerReadonly)m_electricConsumer).SomeOption();
		[DoNotSave(0, null)]
		private IElectricityConsumer m_electricConsumer;

		[DoNotSave(0, null)]
		public Computing ComputingRequired { get; private set; } = Computing.Zero;
		[DoNotSave(0, null)]
		public Option<IComputingConsumerReadonly> ComputingConsumer => ((IComputingConsumerReadonly)m_computingConsumer).SomeOption();

		[DoNotSave(0, null)]
		private IComputingConsumer m_computingConsumer;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationInfoManager;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationErrorManager;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationWarningManager;

		public MaintenanceCosts MaintenanceCosts { get; private set; }

		[DoNotSave(0, null)]
		public IEntityMaintenanceProvider Maintenance => m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private IEntityMaintenanceProvider m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private bool m_reninitNotification;

		[DoNotSave(0, null)]
		private int m_clockSpeed;

		[DoNotSave(0, null)]
		private int m_clock;

		[DoNotSave(0, null)]
		public bool IsIdleForMaintenance => m_maintenanceConsumer.Status.IsBroken;

		[DoNotSave(0, null)]
		public string ErrorMessage { get; private set; }

		[DoNotSave(0, null)]
		public bool IsDebug { get; private set; }

		[DoNotSave(0, null)]
		public bool WaitForUser { get; private set; }

		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT1;
		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT2;
		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT3;

		// TODO handle computation speed quicker than ticks
		public void SimUpdate()
		{
			if (IsNotEnabled && IsNotPaused)
			{
				return;
			}

			if (Modules.Count == 0)
			{
				PowerRequired = Prototype.IddlePower;
				m_electricConsumer.OnPowerRequiredChanged();
				ComputingRequired = Computing.Zero;
				m_computingConsumer.OnComputingRequiredChanged();
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
				return;
			}

			if (IsPaused) {
				return;
			}

			Electricity requiredRunningPower = GetRequiredRunningPower();
			PowerRequired = Prototype.IddlePower + requiredRunningPower;
			m_electricConsumer.OnPowerRequiredChanged();

			Computing requiredComputingPower = GetRequiredComputation();
			ComputingRequired = requiredComputingPower;
			m_computingConsumer.OnComputingRequiredChanged();

			float slotsMaintenance = 0.01f / Prototype.Columns;
			PartialQuantity quantity = new PartialQuantity(
					(Modules.Select(m => m.Layout.GetWidth(m)).Sum() * slotsMaintenance + 0.01f).ToFix32());

			VirtualProductProto lastMaintenanceProduct = Maintenance.Costs.Product;
			VirtualProductProto maintenanceProduct
				= ComputingRequired > 10.TFlops()
				? m_productMaintenanceT3
				: ComputingRequired.IsPositive
				? m_productMaintenanceT2
				: m_productMaintenanceT1;
			var newCosts = new MaintenanceCosts(maintenanceProduct, quantity);
			if (newCosts.MaintenancePerMonth != MaintenanceCosts.MaintenancePerMonth
				|| newCosts.Product != lastMaintenanceProduct)
			{
				MaintenanceCosts = newCosts;
				Maintenance.RefreshMaintenanceCost();
			}

			bool electricityConsumed = m_electricConsumer.TryConsume();

			bool computingConsumed = electricityConsumed && m_computingConsumer.TryConsume();

			if (m_clock >= m_clockSpeed)
			{
				m_clock = 0;
			}
			else
			{
				m_clock++;
				return;
			}

			if (electricityConsumed)
			{
				if (m_maintenanceConsumer.Status.CurrentBreakdownChance < new Random().Next(100).Percent())
				{
					UpdateModules(computingConsumed);
				}
			}
			else
			{
				State = Tr.EntityElectricityConsumptionTooltip__NotEnough;
			}
		}

		private Electricity GetRequiredRunningPower()
		{
			var total = Modules
				.ToArray()
				.Where(m => m.IsNotPaused())
				.Where(m => !(m.Prototype is null))
				.Select(m => m.Prototype.UsedPower.Value)
				.Sum();

			if (Speed == 0) {
				return total.Kw();
			}

			return (total * 1 / (1 + Speed)).Max(1).Kw();
		}

		private Computing GetRequiredComputation()
		{
			PartialQuantity sum = PartialQuantity.Zero;
			foreach (Module module in Modules)
			{
				if (module.IsPaused) {
					continue;
				}
				sum += module.Prototype.UsedComputing;
			}

			if (sum == PartialQuantity.Zero) {
				return Computing.Zero;
			}
			sum = (sum * 1 / (1 + Speed)).Max(PartialQuantity.One);
			return Computing.FromQuantity(sum.IntegerPart.Max(Quantity.One));
		}

		private void UpdateModules(bool computingConsumed)
		{
			// This will run the "compiled tree" per tick
			// the tree is recompiled when edited only or when construct
			Dictionary<long, Module> cache = Modules.ToDictionary(m => m.Id);

			// Copy all outputs to inputs
			foreach (Module module in Modules)
			{
				foreach (var input in module.Prototype.Inputs)
				{
					module.InputNumberData.TryRemove(input.Id, out _);
					// module.StringData.TryRemove("in__" + input.Id, out _);
				}

				foreach (KeyValuePair<string, ModuleConnector> item in module.InputModules.ToArray())
				{
					if (cache.TryGetValue(item.Value.ModuleId, out Module connected))
					{
						module.Input[item.Key] = connected.Output[item.Value.OutputId, 0];
					}
					else
					{
						// Remove disconnected module
						module.InputModules.Remove(item.Key);
						module.Input[item.Key] = Fix32.Zero;
					}
				}
			}

			// Execute all modules
			bool anyError = false;
			bool anyWarning = false;
			bool anyInfo = false;
			bool missingComputation = false;
			foreach (Module module in Modules)
			{
				try
				{
					if (module.Unlocked == false) {
						// Only skip, UI will show automatically
						module.SetStatus(ModuleStatus.Skipped);
						continue;
					}
					if (module.Prototype.UsedComputing > PartialQuantity.Zero && !computingConsumed)
					{
						missingComputation = missingComputation || true;
						module.SetStatus(ModuleStatus.Skipped);
						continue;
					}

					if (module.Status == ModuleStatus.Skipped) {
						module.SetStatus(ModuleStatus.Init);
					}
					module.Execute();
				}
				catch (Exception e)
				{
					anyError = true;
					if (module.IsDebugging) {
						Log.Exception(e);
					}
					// ignore exception
				}
				anyInfo = anyInfo || module.Info;
				anyWarning = anyWarning || module.Warning;
				anyError = anyError || module.Status == ModuleStatus.Error;
			}
			if (anyError)
			{
				m_notificationErrorManager.Activate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
			}
			else if (anyWarning)
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Activate(this);
				m_notificationInfoManager.Deactivate(this);
			}
			else if (anyInfo)
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Activate(this);
			}
			else
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
			}

			State = Tr.EntityStatus__Working;
		}

		public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
		{
			return Quantity.Zero; // TODO keep displayed content
		}

		[DoNotSave()]
		public Lyst<Module> Modules { get; private set; }

		[DoNotSave()]
		public Lyst<Lyst<ModulePlacement>> Rows { get; private set; }

		[DoNotSave()]
		public int GeneralPriority { get; set; }

		[DoNotSave()]
		public bool IsGeneralPriorityVisible => true;

		[DoNotSave()]
		public bool IsCargoAffectedByGeneralPriority => false;

		[DoNotSave()]
		public int Speed { get => m_clockSpeed; set => m_clockSpeed = value; }

		[DoNotSave()]
		public int Clock { get => m_clock; set => m_clock = value; }
		public LocStrFormatted State { get; private set; }
		public ColorRgba Color { get; private set; }

		[DoNotSave(resolveAfterLoad: typeof(ResearchManager))]
		public ResearchManager ResearchManager { get; private set; }
		public void SetColor(ColorRgba color)
		{
			Color = color;
		}
	}
}
