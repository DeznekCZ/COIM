using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Maintenance;
using Mafi.Core.Notifications;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Serialization;
using ProgramableNetwork.Data.Mod;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ProgramableNetwork.Data.DisplayEntity
{
	[ManuallyWrittenSerialization]
	public class DisplayEntity : LayoutEntity, IAreaSelectableEntity, IEntityWithCloneableConfig, IEntityWithSimUpdate,
		IElectricityConsumingEntity, IMaintainedEntity, IUpgradableEntity
	{
		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
		{
			((DisplayEntity)obj).SerializeData(writer);
		};
		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
		{
			((DisplayEntity)obj).DeserializeData(reader);
		};

		public DisplayEntity(EntityId id, DisplayEntityProto proto, TileTransform transform, EntityContext context, IEntityMaintenanceProvidersFactory maintenanceProvidersFactory)
			: base(id, proto, transform, context)
		{
			Prototype = proto;
			ErrorMessage = "";
			m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);
			m_notificationInfoManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.SoundNotification);
			DisplayManager = proto.DisplayManagerFactory(this);
			PropertiesFix32 = new Dict<string, Fix32>();
			m_propertiesChanged = new Lyst<string>();
		}

		[DoNotSave(0, null)]
		private DisplayEntityProto m_proto;
		[DoNotSave(0, null)]
		private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;

		[DoNotSave(0, null)]
		public new DisplayEntityProto Prototype
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

		public override bool CanBePaused => true;
		
		[DoNotSaveCreateNewOnLoad()]
		private Lyst<string> m_propertiesChanged;

		public bool Sync(ref Lyst<string> changes) {
			if (m_propertiesChanged.Count == 0) {
				return false;
			}
			changes.AddRange(m_propertiesChanged);
			m_propertiesChanged.Clear();
			return true;
		}

		public void AddToConfig(EntityConfigData data)
		{
			data.SetBool("isActive", IsActive);
			data.SetArray<KeyValuePair<string, Fix32>>("properties", PropertiesFix32.ToImmutableArray(), (pair, blob) =>
			{
				blob.WriteString(pair.Key);
				Fix32.Serialize(pair.Value, blob);
			});
		}

		public void ApplyConfig(EntityConfigData data)
		{
			IsActive = data.GetBool("isActive") ?? false;

			PropertiesFix32.Clear();
			data.GetArray("properties",
				(blob) => new KeyValuePair<string, Fix32>(blob.ReadString(), Fix32.Deserialize(blob)))
				?.ForEach(pair =>
				{
					PropertiesFix32[pair.Key] = pair.Value;
				});
		}

		public static void Serialize(DisplayEntity value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value))
			{
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		public static DisplayEntity Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out DisplayEntity value, null))
			{
				reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
			}
			return value;
		}

		[InitAfterLoad(InitPriority.Normal)]
		[OnlyForSaveCompatibility(null)]
		private void initContexts(int saveVersion)
		{
			Log.Info($"Initialize context after load");

			Prototype = Context.ProtosDb.Get<DisplayEntityProto>(m_protoId).ValueOrThrow("Invalid antene proto: " + m_protoId);
			m_electricConsumer = m_electricConsumer ?? Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_notificationInfoManager = WithId(ControllerNotification.SoundNotification, m_notificationInfoManager);
			DisplayManager = Prototype.DisplayManagerFactory(this);
			m_propertiesChanged = new Lyst<string>();
		}

		private EntityNotificator WithId(EntityNotificationProto.ID newNotification, EntityNotificator notification)
		{
			PropertyInfo field = typeof(EntityNotificator).GetProperty("NotificationId");
			object v = Context.NotificationsManager.CreateNotificatorFor(newNotification);
			field.SetValue(v, notification.NotificationId);
			return (EntityNotificator)v;
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			writer.WriteString(m_protoId.Value);
			writer.WriteInt(/* Version */4);

			writer.WriteString(ErrorMessage ?? "");
			writer.WriteBool(IsActive);

			writer.WriteGeneric(m_maintenanceConsumer);
			writer.WriteGeneric(m_electricConsumer);

			Dict<string, Fix32>.Serialize(PropertiesFix32, writer);
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			m_protoId = new StaticEntityProto.ID(reader.ReadString());
			int version = reader.ReadInt();

			ErrorMessage = reader.ReadString();
			IsActive = reader.ReadBool();

			m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();
			m_electricConsumer = reader.ReadGenericAs<IElectricityConsumer>();

			if (version == 3)
			{
				reader.ReadGenericAs<IUpgrader>();
			}

			PropertiesFix32 = Dict<string, Fix32>.Deserialize(reader);

			reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
		}

		[DoNotSave(0, null)]
		public Upoints MonthlyUnityConsumed => 0.Upoints();

		[DoNotSave(0, null)]
		public Upoints MaxMonthlyUnityConsumed => 0.Upoints();

		public Proto.ID UpointsCategoryId => IdsCore.UpointsCategories.Boost;

		[DoNotSave(0, null)]
		private EntityNotificator m_notificationInfoManager;
		[DoNotSave(0, null)]
		public IDisplayEntityManager DisplayManager { get; private set; }

		[DoNotSave(0, null)]
		public int CurrentInstruction { get; private set; }

		[DoNotSave(0, null)]
		public Electricity PowerRequired { get; private set; } = Electricity.Zero;

		[DoNotSave(0, null)]
		public Option<IElectricityConsumerReadonly> ElectricityConsumer => ((IElectricityConsumerReadonly)m_electricConsumer).SomeOption();
		[DoNotSave(0, null)]
		private IElectricityConsumer m_electricConsumer;

		public MaintenanceCosts MaintenanceCosts { get; private set; }

		[DoNotSave(0, null)]
		public IEntityMaintenanceProvider Maintenance => m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private IEntityMaintenanceProvider m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private long m_nextPlay;
		private IUpgrader m_upgrader;

		[DoNotSave(0, null)]
		public bool IsIdleForMaintenance => m_maintenanceConsumer.Status.IsBroken;

		[DoNotSave(0, null)]
		public string ErrorMessage { get; private set; }

		[DoNotSave(0, null)]
		public bool IsDebug { get; private set; }

		[DoNotSave(0, null)]
		public bool WaitForUser { get; private set; }

		public void SimUpdate()
		{
			if (IsNotEnabled && IsNotPaused)
			{
				return;
			}

			if (IsPaused || m_maintenanceConsumer.Status.IsBroken)
			{
				CurrentInstruction = 0;
				PowerRequired = Electricity.Zero;
				return;
			}

			PowerRequired = IsActive ? Prototype.WorkingPower : Prototype.IddlePower;
			m_electricConsumer.OnPowerRequiredChanged();

			if (!m_electricConsumer.TryConsume())
			{
				return;
			}
		}

		[DoNotSave()]
		public override bool IsGeneralPriorityVisible => true;

		[DoNotSave()]
		public override bool IsCargoAffectedByGeneralPriority => false;

		[DoNotSave()]
		public bool IsActive { get; private set; }

		public void SetActive(bool isActive)
		{
			IsActive = isActive;
		}

		[DoNotSave()]
		public Dict<string, Fix32> PropertiesFix32 { get; private set; }

		public void SetProperty(string name, Fix32 value)
		{
			PropertiesFix32[name] = value;
			m_propertiesChanged.Add(name);
		}

		public Fix32 GetProperty(string name, Fix32? defaultValue = null)
		{
			return PropertiesFix32.TryGetValue(name, out Fix32 value) ? value : (defaultValue ?? Fix32.Zero);
		}

		public bool IsUpgradeAvailable(out LocStrFormatted errorMessage)
		{
			errorMessage = LocStrFormatted.Empty;
			return true;
		}

		public bool TryReplaceSelf(IProtoWithUpgrade newProto, bool dryRun, out LocStrFormatted errorMessage)
		{
			errorMessage = LocStrFormatted.Empty;

			if (newProto is not DisplayEntityProto display)
			{
				errorMessage = Tr.EntityStatus__InvalidPlacement;
				return false;
			}

			if ((Prototype.Upgrade.NextTier.ValueOrNull is DisplayEntityProto next && next != display)
				&& (Prototype.Upgrade.PreviousTier.ValueOrNull is DisplayEntityProto prev && prev != display))
			{
				errorMessage = Tr.EntityStatus__InvalidPlacement;
				return false;
			}

			if (dryRun == false)
			{
				Prototype = display;
				DisplayManager = Prototype.DisplayManagerFactory(this);
			}

			return true;
		}

		public IProtoWithUpgrade UpgradableProto => Prototype;
	}
}
