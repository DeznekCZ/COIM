using Mafi;
using Mafi.Collections;
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
using Mafi.Core.Products;
using Mafi.Core.Terrain;
using Mafi.Core.Terrain.Designation;
using Mafi.Depedencies;
using Mafi.Serialization;
using System;
using UnityEngine;

namespace BucketWheelExcavator.Entity
{
    [GenerateSerializer(false, null, 0)]
    public class BucketWheelExcavator : LayoutEntity, IEntityWithGeneralPriority, IEntityWithWorkers, IMaintainedEntity, IEntityWithPorts, IEntityWithSimUpdate, IEntity/*, IEntityConstructionProgress*/
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
        private Queueue<PartialProductQuantity> m_queue;
        [DoNotSave(0, null)]
        private Dict<ProductSlimId, PartialQuantity> m_overflow;

        [DoNotSave(0, null)]
        public new BucketWheelExcavatorProto Prototype
        {
            get
            {
                return m_proto;
            }
        }

        public BucketWheelExcavator(EntityId id, BucketWheelExcavatorProto proto, TileTransform tileTransform, EntityContext context, IEntityMaintenanceProvidersFactory maintenanceProvidersFactory)
            : base(id, proto, tileTransform, context)
        {
            this.m_proto = proto;
            this.m_protoId = proto.Id;
            this.MaintenanceCosts = Prototype.Costs.Maintenance;
            this.m_maintenance = maintenanceProvidersFactory.CreateFor(this);
            this.m_queue = new Queueue<PartialProductQuantity>();
            this.m_overflow = new Dict<ProductSlimId, PartialQuantity>();

            // init queue
            for (int i = 0; i < 10; i++)
            {
                m_queue.Enqueue(PartialProductQuantity.None);
            }

            this.Height = (-60f + 15f).ToFix32();
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

        [DoNotSave(0, null)]
        public Lyst<Tile3f> Buckets { get; set; }

        protected override bool IsEnabledNow => IsNotPaused || m_queue.Count > 10;

        [DoNotSave(0, null)]
        public Queueue<PartialProductQuantity> Queue => m_queue;

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
            writer.WriteInt(/* Version */2);
            writer.WriteGeneric(m_maintenance);
            writer.WriteInt(Distance.RawValue);
            writer.WriteInt(Direction.RawValue);
            writer.WriteInt(Height.RawValue);

            Queueue<PartialProductQuantity>.Serialize(m_queue, writer);
            Dict<ProductSlimId, PartialQuantity>.Serialize(m_overflow, writer);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            m_protoId = new LayoutEntityProto.ID(reader.ReadString());
            int version = reader.ReadInt();
            m_maintenance = reader.ReadGenericAs<IEntityMaintenanceProvider>();

            if (version >= 2)
            {
                Distance = Fix32.FromRaw(reader.ReadInt());
                Direction = Fix32.FromRaw(reader.ReadInt());
                Height = Fix32.FromRaw(reader.ReadInt());
            }

            m_queue = Queueue<PartialProductQuantity>.Deserialize(reader);
            m_overflow = Dict<ProductSlimId, PartialQuantity>.Deserialize(reader);

            reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
        }

        public void SimUpdate()
        {
            if (IsEnabledNow && ConstructionState == ConstructionState.Constructed)
            {
                TryMoveBelt();
                TryMine();
            }
        }

        private void TryMine()
        {
            if (m_queue.Count < 10)
            {
                Log.Info("Mine in progress");
                var terrain = GlobalDependencyResolver.Get<TerrainManager>();
                var slimIdManager = GlobalDependencyResolver.Get<ProductsSlimIdManager>();
                Lyst<PartialProductQuantity> mined = new Lyst<PartialProductQuantity>();
                foreach (Tile3f item in Buckets)
                {
                    HeightTilesF height = terrain.GetHeight(item.Tile2i);
                    if (height < item.Height)
                        continue;

                    TerrainMaterialThicknessSlim minedMaterial = terrain.MineMaterial(
                        new Tile2iAndIndex(item.Tile2i.AsSlim, terrain.GetTileIndex(item.Tile2i).Value),
                        ThicknessTilesF.One);

                    PartialProductQuantity quantity = minedMaterial.ToPartialProductQuantity(terrain);
                    if (quantity.Quantity > Quantity.Zero)
                    {
                        m_queue.Enqueue(quantity);
                        return;
                    }
                }

                m_queue.Enqueue(PartialProductQuantity.None);

                Direction += 0.5f.ToFix32();
                if (Direction >= 360)
                {
                    Direction = 0;
                    Height -= 0.5f.ToFix32();
                }
            }
        }

        private void TryMoveBelt()
        {
            if (m_queue.Peek().IsNotEmpty)
            {
                if (ConnectedOutputPorts.First.IsConnected)
                {
                    PartialProductQuantity quantity = m_queue.Dequeue();
                    if (m_overflow.TryGetValue(quantity.Product.SlimId, out PartialQuantity overflow))
                    {
                        quantity += overflow;
                        m_overflow.Remove(quantity.Product.SlimId);
                    }

                    ProductQuantity fullPart = new ProductQuantity(quantity.Product, quantity.Quantity.IntegerPart);
                    PartialQuantity fraction = quantity.Quantity.FractionalPart;
                    Quantity rest = ConnectedOutputPorts.First.SendAsMuchAs(fullPart);

                    if (rest > Quantity.Zero)
                    {
                        m_queue.EnqueueAt(new PartialProductQuantity(quantity.Product, rest.AsPartial + fraction), 0);
                    }

                    if (fraction > PartialQuantity.Zero)
                    {
                        m_overflow[quantity.Product.SlimId] = fraction;
                    }
                }
            }
            else
            {
                // dequeue belt slot
                m_queue.Dequeue();
            }
        }

        public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
        {
            return Quantity.Zero;
        }
    }
}
