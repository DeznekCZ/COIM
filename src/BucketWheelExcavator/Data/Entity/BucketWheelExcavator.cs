using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Priorities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Environment;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Maintenance;
using Mafi.Core.Population;
using Mafi.Core.Ports;
using Mafi.Core.Ports.Io;
using Mafi.Core.Terrain;
using Mafi.Depedencies;
using Mafi.Serialization;
using System;

namespace BucketWheelExcavator.Entity
{
    [GenerateSerializer(false, null, 0)]
    public class BucketWheelExcavator : LayoutEntity, IEntityWithGeneralPriority, IEntityWithWorkers, IMaintainedEntity, IEntityWithPorts
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((BucketWheelExcavator)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((BucketWheelExcavator)obj).DeserializeData(reader);
        };

        [DoNotSave(0, null)]
        private IEntityMaintenanceProvider m_maintenance;
        [DoNotSave(0, null)]
        private BucketWheelExcavatorProto m_proto;
        [DoNotSave(0, null)]
        private StaticEntityProto.ID m_protoId;

        [DoNotSave(0, null)]
        public new BucketWheelExcavatorProto Prototype
        {
            get
            {
                return m_proto;
            }
        }

        public BucketWheelExcavator(EntityId id, BucketWheelExcavatorProto proto, TileTransform tileTransform, EntityContext context, TerrainManager terrain, IEntityMaintenanceProvidersFactory maintenanceProvidersFactory)
            : base(id, proto, tileTransform, context)
        {
            this.m_proto = proto;
            this.m_protoId = proto.Id;
            this.MaintenanceCosts = Prototype.Costs.Maintenance;
            this.m_maintenance = maintenanceProvidersFactory.CreateFor(this);
        }

        public override bool CanBePaused => true;

        [DoNotSave(0, null)]
        public MaintenanceCosts MaintenanceCosts { get; private set; }

        public bool IsIdleForMaintenance => false;

        public int WorkersNeeded => (base.Prototype as BucketWheelExcavatorProto).WorkersNeeded;

        public bool HasWorkersCached { get; set; }

        public IEntityMaintenanceProvider Maintenance => m_maintenance;

        public Fix32 Direction { get; set; }
        public Fix32 Distance { get; set; }
        public Fix32 Height { get; set; }

        public static void Serialize(BucketWheelExcavator value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static BucketWheelExcavator Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out BucketWheelExcavator value, null))
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

            m_proto = Context.ProtosDb.Get<BucketWheelExcavatorProto>(m_protoId).ValueOrThrow("Invalid bucket wheel excavator proto: " + m_protoId);
            base.Prototype = m_proto;
            MaintenanceCosts = Prototype.Costs.Maintenance;
        }

        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteString(m_protoId.Value);
            writer.WriteInt(/* Version */1);
            writer.WriteGeneric(m_maintenance);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            m_protoId = new LayoutEntityProto.ID(reader.ReadString());
            int version = reader.ReadInt();
            m_maintenance = reader.ReadGenericAs<IEntityMaintenanceProvider>();

            reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
        }

        public void SimUpdate()
        {
            
        }

        public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
        {
            return Quantity.Zero;
        }
    }
}
