using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Ports.Io;
using System;
using System.Linq;
using Mafi.Collections.ImmutableCollections;
using Mafi.Serialization;
using Mafi.Core.Population;
using Mafi.Core.Prototypes;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Maintenance;
using Mafi.Core.Entities.Static;
using Mafi.Core.World;
using static ProgramableNetwork.DataBands;

namespace ProgramableNetwork
{
    [ManuallyWrittenSerialization]
    public class Antena : LayoutEntityBase, IAreaSelectableEntity, IEntityWithCloneableConfig, IEntityWithSimUpdate,
        IUnityConsumingEntity, IComputingConsumingEntity, IElectricityConsumingEntity, IMaintainedEntity
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((Antena)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((Antena)obj).DeserializeData(reader);
        };

        public Option<string> CustomTitle { get; set; }
        public IDataBand DataBand { get; set; }

        public Antena(EntityId id, AntenaProto proto, TileTransform transform, EntityContext context,
			IEntityMaintenanceProvidersFactory maintenanceProvidersFactory, RandomProvider randomProvider,
			IWorldMapManager worldMapManager)
            : base(id, proto, transform, context)
        {
            Prototype = proto;
            ErrorMessage = "";
            m_unityConsumer = Context.UnityConsumerFactory.CreateConsumer(this);
            m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
            m_computingConsumer = Context.ComputingConsumerFactory.CreateConsumer(this);
            m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);
			WorldMapManager = worldMapManager;
			RandomProvider = randomProvider;

            DataBand = new UnkownnDataBandType(Context, Context.ProtosDb.Get<DataBandProto>(DataBand_Unknown).ValueOrThrow("Unknown signal not found"), this);
        }

        [DoNotSave(0, null)]
        private AntenaProto m_proto;
        [DoNotSave(0, null)]
        private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;

        [DoNotSave(0, null)]
        public new AntenaProto Prototype
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

        public void AddToConfig(EntityConfigData data)
        {
            data.SetString("databand_type", DataBand.Prototype.Id.Value);

            // AM channel routing (mine/ship → channel-slot mappings) used to be
            // dropped on the floor by the game's "copy entity config" Ctrl+C/V flow
            // because only the band TYPE was stored — the new antena came up with
            // an empty redirected list.  Pack each redirected channel as a flat
            // (index, sourceEntityId, operation) triplet so ApplyConfig can rebuild
            // m_redirected and re-resolve the WorldMapMine / BattleShip refs.
            if (DataBand is AMDataBand am)
            {
                var channels = am.Channels.OfType<AMDataBandChannel>().ToList();
                int[] packed = new int[channels.Count * 3];
                for (int i = 0; i < channels.Count; i++)
                {
                    var c = channels[i];
                    packed[i * 3 + 0] = c.Index;
                    packed[i * 3 + 1] = c.SourceIdForConfig;
                    packed[i * 3 + 2] = (int)c.Operation;
                }
                data.SetArray<int>("am_channels", ImmutableArray.Create(packed),
                    (v, w) => w.WriteInt(v));
            }
        }

        public void ApplyConfig(EntityConfigData data)
        {
            string id = data.GetString("databand_type").ValueOrNull ?? DataBands.DataBand_Unknown.Value;
            Proto.ID dataBandType = new Proto.ID(id);
            DataBandProto dataBandProto = Context.ProtosDb.Get<DataBandProto>(dataBandType).ValueOrNull;
            DataBand = dataBandProto.Constructor(this, Context, dataBandProto);

            // Restore AM channel routing if the source antena was an AM band (see
            // AddToConfig).  When cloning across band types (e.g. FM source → AM
            // destination) the key is absent and m_redirected stays empty, which is
            // the correct fallback.
            if (DataBand is AMDataBand am)
            {
                var packed = data.GetArray<int>("am_channels", r => r.ReadInt());
                if (packed.HasValue)
                {
                    am.RestoreFromConfig(packed.Value, Context.EntitiesManager);
                }
            }
        }

        public static void Serialize(Antena value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static Antena Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out Antena value, null))
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

            WorldMapManager = resolver.Resolve<IWorldMapManager>();
            RandomProvider = resolver.Resolve<RandomProvider>();

			Prototype = Context.ProtosDb.Get<AntenaProto>(m_protoId).ValueOrThrow("Invalid antene proto: " + m_protoId);
            m_electricConsumer = m_electricConsumer ?? Context.ElectricityConsumerFactory.CreateConsumer(this);
            m_computingConsumer = m_computingConsumer ?? Context.ComputingConsumerFactory.CreateConsumer(this);

            if (DataBand == null)
            {
                DataBand = new UnkownnDataBandType(Context, Context.ProtosDb.Get<DataBandProto>(DataBand_Unknown).ValueOrThrow("Unknown signal not found"), this);
            }
            else
            {
                Log.Info($"Loaded DataBand type: {DataBand.GetType().FullName}");
                DataBand = (DataBand as UnloadedDataBand).Deserialize(Context, saveVersion);
                DataBand.Context = Context;
                DataBand.initContext(this);
                Log.Info($"Deserialized DataBand type: {DataBand.GetType().FullName}");
            }
        }

        private static readonly int SerializerVersion = 1;
        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteString(m_protoId.Value);
            writer.WriteInt(SerializerVersion);

            writer.WriteString(ErrorMessage ?? "");
            Option<string>.Serialize(CustomTitle, writer);

            writer.WriteInt(GeneralPriority);
            writer.WriteGeneric(m_maintenanceConsumer);
            writer.WriteGeneric(m_electricConsumer);
            writer.WriteGeneric(m_computingConsumer);

            DataBand.Serialize(writer);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            m_protoId = new StaticEntityProto.ID(reader.ReadString());
            int version = reader.ReadInt();

            ErrorMessage = reader.ReadString();
            CustomTitle = Option<string>.Deserialize(reader);

            GeneralPriority = reader.ReadInt();
            m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();

            if (version >= 1)
            {
                m_electricConsumer = reader.ReadGenericAs<IElectricityConsumer>();
                m_computingConsumer = reader.ReadGenericAs<IComputingConsumer>();
            }

            DataBand = reader.ReadDataBand();

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
        public bool Selected { get; set; }

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

        public MaintenanceCosts MaintenanceCosts { get; private set; }

        [DoNotSave(0, null)]
        public IEntityMaintenanceProvider Maintenance => m_maintenanceConsumer;
        [DoNotSave(0, null)]
        private IEntityMaintenanceProvider m_maintenanceConsumer;
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
            m_electricConsumer.OnPowerRequiredChanged();
            if (IsNotEnabled && IsNotPaused)
            {
                return;
            }

            if (!m_electricConsumer.TryConsume())
            {
                return;
            }

            if (IsPaused || m_maintenanceConsumer.Status.IsBroken)
            {
                CurrentInstruction = 0;
                PowerRequired = Electricity.Zero;
                if (!IsPaused) {
					Maintenance.SetCurrentMaintenanceTo(Percent.Zero);
				}
				return;
            }

            PowerRequired = Prototype.IddlePower + DataBand.RequiredPower;

            ComputingRequired = DataBand.RequiredComputation;

            DataBand.Update();
        }

        public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
        {
            return Quantity.Zero; // TODO keep displayed content
        }

        [DoNotSave()]
        public int GeneralPriority { get; set; }

        [DoNotSave()]
        public bool IsGeneralPriorityVisible => true;

        [DoNotSave()]
        public bool IsCargoAffectedByGeneralPriority => false;
		[DoNotSave(resolveAfterLoad:typeof(IWorldMapManager))]
		public IWorldMapManager WorldMapManager { get; private set; }
		[DoNotSave(resolveAfterLoad:typeof(RandomProvider))]
		public RandomProvider RandomProvider { get; private set; }
	}
}
