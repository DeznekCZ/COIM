using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Ports.Io;
using System;
using Mafi.Serialization;
using Mafi.Core.Population;
using Mafi.Core.Prototypes;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Maintenance;
using Mafi.Core.Entities.Static;
using static ProgramableNetwork.DataBands;
using Mafi.Core.Notifications;
using ProgramableNetwork.Data.Mod;
using System.Reflection;
using Mafi.Unity.Audio;
using Mafi.Unity;
using Mafi.Unity.Ui;
using UnityEngine;

namespace ProgramableNetwork.Data.Speaker
{
    [ManuallyWrittenSerialization]
    public class Speaker : LayoutEntityBase, IAreaSelectableEntity, IEntityWithCloneableConfig, IEntityWithSimUpdate,
        IElectricityConsumingEntity, IMaintainedEntity
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((Speaker)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((Speaker)obj).DeserializeData(reader);
        };

        public Option<string> CustomTitle { get; set; }

        public Speaker(EntityId id, SpeakerProto proto, TileTransform transform, EntityContext context,
         IEntityMaintenanceProvidersFactory maintenanceProvidersFactory, DependencyResolver resolver)
            : base(id, proto, transform, context)
        {
            Prototype = proto;
            ErrorMessage = "";
            Sound = Assets.Unity.UserInterface.Audio.ShipAlarm_prefab;
            Volume = Percent.Hundred;
            m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
            m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);
            m_notificationInfoManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.SoundNotification);
            Resolver = resolver;
        }

        [DoNotSave(0, null)]
        private SpeakerProto m_proto;
        [DoNotSave(0, null)]
        private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;

        [DoNotSave(0, null)]
        public new SpeakerProto Prototype
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

        [DoNotSave(resolveAfterLoad:typeof(DependencyResolver))]
        public DependencyResolver Resolver { get; private set; }

		public void AddToConfig(EntityConfigData data)
        {
            data.SetBool("isPlaying", IsPlaying);
            data.SetString("sound", Sound);
        }

        public void ApplyConfig(EntityConfigData data)
        {
            IsPlaying = data.GetBool("isPlaying") ?? false;
            Sound = data.GetString("sound").ValueOrNull ?? Assets.Unity.UserInterface.Audio.ShipAlarm_prefab;
        }

        public static void Serialize(Speaker value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static Speaker Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out Speaker value, null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        [InitAfterLoad(InitPriority.Normal)]
        [OnlyForSaveCompatibility(null)]
        private void initContexts(int saveVersion, DependencyResolver resolver)
        {
            Resolver = resolver;
			Log.Info($"Initialize context after load");

            Prototype = Context.ProtosDb.Get<SpeakerProto>(m_protoId).ValueOrThrow("Invalid antene proto: " + m_protoId);
            m_electricConsumer = m_electricConsumer ?? Context.ElectricityConsumerFactory.CreateConsumer(this);
            m_notificationInfoManager = WithId(ControllerNotification.SoundNotification, m_notificationInfoManager);
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
            writer.WriteInt(/* Version */2);

            writer.WriteString(ErrorMessage ?? "");
            Option<string>.Serialize(CustomTitle, writer);
            writer.WriteString(Sound ?? Assets.Unity.UserInterface.Audio.ShipAlarm_prefab);
            writer.WriteInt(Volume.RawValue);
            writer.WriteBool(IsPlaying);

            writer.WriteInt(GeneralPriority);
            writer.WriteGeneric(m_maintenanceConsumer);
            writer.WriteGeneric(m_electricConsumer);

            // specific settings
            writer.WriteUInt(m_notificationInfoManager.NotificationId.Value);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            m_protoId = new StaticEntityProto.ID(reader.ReadString());
            int version = reader.ReadInt();

            ErrorMessage = reader.ReadString();
            CustomTitle = Option<string>.Deserialize(reader);

            if (version < 2)
            {
                if (version < 1)
                {
                    Sound = Assets.Unity.UserInterface.Audio.ShipAlarm_prefab;
                    Volume = Percent.Hundred;
                    IsPlaying = false;
                }
                else
                {
                    Sound = reader.ReadString() ?? Assets.Unity.UserInterface.Audio.ShipAlarm_prefab;
                    Volume = Percent.Hundred;
                    IsPlaying = reader.ReadBool();
                }
            }
            else
            {
                Sound = reader.ReadString() ?? Assets.Unity.UserInterface.Audio.ShipAlarm_prefab;
                Volume = Percent.FromRaw(reader.ReadInt());
                IsPlaying = reader.ReadBool();
            }

            GeneralPriority = reader.ReadInt();
            m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();
            m_electricConsumer = reader.ReadGenericAs<IElectricityConsumer>();

            // specific settings
            object v = m_notificationInfoManager = new EntityNotificator();
            typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
            m_notificationInfoManager = (EntityNotificator)v;

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
                m_notificationInfoManager.Deactivate(this);
                return;
            }

            PowerRequired = IsPlaying ? Prototype.WorkingPower : Prototype.IddlePower;
            m_electricConsumer.OnPowerRequiredChanged();

            if (!m_electricConsumer.TryConsume())
            {
                m_notificationInfoManager.Deactivate(this);
                return;
            }

            if (IsPaused || m_maintenanceConsumer.Status.IsBroken)
            {
                CurrentInstruction = 0;
                PowerRequired = Electricity.Zero;
                m_notificationInfoManager.Deactivate(this);
                return;
            }

            if (IsPlaying)
            {
                m_notificationInfoManager.Activate(this);

                if (m_nextPlay < DateTime.Now.Ticks || m_nextPlay == 0)
                {
                    var clipper = Resolver.Resolve<UiContext>().AudioDb.GetSharedAudioUi(Sound);

                    AudioSource.PlayClipAtPoint(clipper.clip, Position3f.ToVector3(), Volume.ToFloat() * 100f);
                    m_nextPlay = DateTime.Now.Ticks + (long)(clipper.clip.length * TimeSpan.TicksPerSecond);
                }
            }
            else
            {
                m_notificationInfoManager.Deactivate(this);
            }
        }

        [DoNotSave()]
        public int GeneralPriority { get; set; }

        [DoNotSave()]
        public bool IsGeneralPriorityVisible => true;

        [DoNotSave()]
        public bool IsCargoAffectedByGeneralPriority => false;

        [DoNotSave()]
        public bool IsPlaying { get; private set; }

        public void SetPlaying(bool isPlaying)
        {
            IsPlaying = isPlaying;
        }

        [DoNotSave()]
        public string Sound { get; private set; }

        public void SetSound(string sound)
        {
            Sound = sound;
        }

        [DoNotSave()]
        public Percent Volume { get; private set; }

        public void SetVolume(Percent volume)
        {
            Volume = volume;
        }
    }
}
