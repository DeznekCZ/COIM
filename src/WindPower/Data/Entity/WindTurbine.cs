using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Environment;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Maintenance;
using Mafi.Depedencies;
using Mafi.Serialization;
using System;
using WindPower.Simulation;

namespace WindPower.Entity
{
    [GenerateSerializer(false, null, 0)]
    public class WindTurbine : LayoutEntityBase, IElectricityGeneratingEntity, IMaintainedEntity, IEntityWithSimUpdate
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((WindTurbine)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((WindTurbine)obj).DeserializeData(reader);
        };

        [DoNotSave(0, null)]
        private WeatherManager m_weatherManager;
        [DoNotSave(0, null)]
        private IElectricityGeneratorRegistrator m_generator;
        [DoNotSave(0, null)]
        private IEntityMaintenanceProvider m_maintenance;
        [DoNotSave(0, null)]
        private WindTurbineProto m_proto;
        [DoNotSave(0, null)]
        private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;
        private bool m_canGenerate;
        private Electricity m_power;

        [DoNotSave(0, null)]
        public new WindTurbineProto Prototype
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

        public WindTurbine(EntityId id, WindTurbineProto proto, TileTransform transform, EntityContext context, WeatherManager weatherManager,
            IElectricityGeneratorRegistratorFactory generatorRegistratorFactory, IEntityMaintenanceProvidersFactory maintenanceProvidersFactory)
            : base(id, proto, transform, context)
        {
            this.Prototype = proto;
            this.m_weatherManager = weatherManager;
            this.m_generator = generatorRegistratorFactory.CreateAndRegisterFor(this, 0);
            this.MaintenanceCosts = Prototype.Costs.Maintenance;
            this.m_maintenance = maintenanceProvidersFactory.CreateFor(this);
            MaxGenerationCapacity = Prototype.GeneratedPower;
        }

        public IElectricityGeneratorRegistrator ElectricityGenerator => m_generator;

        [DoNotSave(0, null)]
        public Electricity MaxGenerationCapacity { get; private set; }

        public override bool CanBePaused => true;

        [DoNotSave(0, null)]
        public Percent StoredPower { get; private set; }
        [DoNotSave(0, null)]
        public Percent TargetPower { get; private set; }
        [DoNotSave(0, null)]
        public Percent Speed { get; private set; }
        [DoNotSave(0, null)]
        public Fix32 WindDirection { get; private set; }
        [DoNotSave(0, null)]
        public MaintenanceCosts MaintenanceCosts { get; private set; }

        public IEntityMaintenanceProvider Maintenance => m_maintenance;

        public bool IsIdleForMaintenance => false;

        public int GeneralPriority => 0;

        public bool IsGeneralPriorityVisible => false;

        public bool IsCargoAffectedByGeneralPriority => false;

        [DoNotSave(0, null)]
        public Percent WindPower { get; private set; }

        public Electricity GenerateAsMuchAs(Electricity freeCapacity, Electricity currentMaxGeneration)
        {
            return currentMaxGeneration; // always generate full if not managed, TODO
        }

        public Electricity GetCurrentMaxGeneration(out bool canGenerate)
        {
            canGenerate = m_canGenerate;
            return m_power;
        }

        public static void Serialize(WindTurbine value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static WindTurbine Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out WindTurbine value, null))
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

            Prototype = Context.ProtosDb.Get<WindTurbineProto>(m_protoId).ValueOrThrow("Invalid wind turbine proto: " + m_protoId);
            MaxGenerationCapacity = Prototype.GeneratedPower;
            m_weatherManager = resolver.Resolve<WeatherManager>();
        }

        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteString(m_protoId.Value);
            writer.WriteInt(/* Version */1);
            Percent.Serialize(StoredPower, writer);
            Percent.Serialize(TargetPower, writer);
            Percent.Serialize(Speed, writer);
            Fix32.Serialize(WindDirection, writer);
            writer.WriteGeneric(m_generator);
            writer.WriteGeneric(m_maintenance);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            m_protoId = new StaticEntityProto.ID(reader.ReadString());
            int version = reader.ReadInt();
            StoredPower = Percent.Deserialize(reader);
            TargetPower = Percent.Deserialize(reader);
            Speed = Percent.Deserialize(reader);
            if (version > 0)
                WindDirection = Fix32.Deserialize(reader);
            m_generator = reader.ReadGenericAs<IElectricityGeneratorRegistrator>();
            m_maintenance = reader.ReadGenericAs<IEntityMaintenanceProvider>();

            reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
        }

        public void SimUpdate()
        {
            var windMap = GlobalDependencyResolver.Get<WindMap>();
            WindDirection = windMap.GetWindDirection();

            WindPower = windMap.GetWindPower(CenterTile.SetZ(Position3f.Tile3i.Z), Prototype.GondolaHeight, Prototype.BladeWidth);
            if (!IsEnabled)
            {
                TargetPower = Percent.Zero;
            }
            else
            {
                TargetPower = WindPower.Clamp0To100();
            }

            if (TargetPower == Percent.Zero && StoredPower == Percent.Zero)
            {
                m_canGenerate = false;
                Speed = StoredPower;
                m_power = Electricity.Zero;
                MaxGenerationCapacity = Prototype.GeneratedPower.Min(m_power);
                return;
            }

            else if (TargetPower > StoredPower)
            {
                float powerUnit = Prototype.BrakingPower.Value * WindPower.ToFloat() / Prototype.GeneratedPower.Value * 0.0625f;

                StoredPower += Percent.FromFloat(powerUnit);
                StoredPower = StoredPower.Min(TargetPower).Clamp0To100();

                m_canGenerate = StoredPower > Percent.Zero;
                if (!m_canGenerate)
                {
                    Speed = StoredPower;
                    m_power = Electricity.Zero;
                    MaxGenerationCapacity = Prototype.GeneratedPower.Min(m_power);
                    return;
                }

                Speed = StoredPower;
                float partial = StoredPower.ToFloat() / Prototype.GeneratedPower.Quantity.Value;
                m_power = Electricity.FromKw((int)(Prototype.GeneratedPower.Value * partial));
                MaxGenerationCapacity = Prototype.GeneratedPower.Min(m_power);
                return;
            }

            else if (TargetPower < StoredPower)
            {
                float windPowerMultiplier = WindPower == Percent.Zero ? 1 : WindPower.ToFloat();
                float powerUnit = Prototype.BrakingPower.Value / windPowerMultiplier / Prototype.GeneratedPower.Value * 0.0625f;

                StoredPower -= Percent.FromFloat(powerUnit);
                StoredPower = StoredPower.Max(TargetPower).Clamp0To100();

                m_canGenerate = StoredPower > Percent.Zero;
                if (!m_canGenerate)
                {
                    Speed = StoredPower;
                    m_power = Electricity.Zero;
                    MaxGenerationCapacity = Prototype.GeneratedPower.Min(m_power);
                    return;
                }
                if (StoredPower == Percent.Hundred)
                {
                    Speed = StoredPower;
                    m_power = Prototype.GeneratedPower;
                    MaxGenerationCapacity = Prototype.GeneratedPower.Min(m_power);
                    return;
                }

                float partial = StoredPower.ToFloat() / Prototype.GeneratedPower.Quantity.Value;
                Speed = StoredPower;
                m_canGenerate = true;
                m_power = Electricity.FromKw((int)(Prototype.GeneratedPower.Value * partial));
                MaxGenerationCapacity = Prototype.GeneratedPower.Min(m_power);
                return;
            }

            // else power is same
            m_canGenerate = true;
            Speed = StoredPower;
            m_power = Prototype.GeneratedPower;
            MaxGenerationCapacity = Prototype.GeneratedPower.Min(m_power);
        }
    }
}
