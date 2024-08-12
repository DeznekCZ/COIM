using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Priorities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Products;
using Mafi.Core.Vehicles;
using Mafi.Serialization;
using System;
using System.Linq;

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    internal class MultiplayerTradeDock : LayoutEntity, IEntityWithCustomPriority, IEntity, IIsSafeAsHashKey, IStaticEntityWithReservedOcean, ILayoutEntity, IStaticEntity, IEntityWithPosition, IRenderedEntity, IAreaSelectableEntity, IEntityWithSimUpdate
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((MultiplayerTradeDock)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((MultiplayerTradeDock)obj).DeserializeData(reader);
        };

        public string Address { get; set; } = string.Empty;
        public string Authorization { get; set; } = string.Empty;
        public ReservedOceanAreaState ReservedOceanAreaState
        {
            get;
            private set;
        }
        public IProtoWithReservedOcean ReservedOceanProto { get; private set; }

        public override bool CanBePaused => false;

        private readonly IVehicleBuffersRegistry m_vehicleBuffersRegistry;


        [DoNotSave(0, null)]
        public new MultiplayerTradeDockProto Prototype
        {
            get
            {
                return m_proto;
            }
            protected set
            {
                m_proto = value;
                base.Prototype = value;
            }
        }

        public MultiplayerTradeDock(EntityId id, MultiplayerTradeDockProto proto, TileTransform transform, EntityContext context, IVehicleBuffersRegistry vehicleBuffersRegistry)
            : base(id, proto, transform, context)
        {
            Prototype = proto;
            ReservedOceanProto = proto;
            ReservedOceanAreaState = new ReservedOceanAreaState(proto, this, IdsCore.Notifications.OceanAccessBlocked, context.NotificationsManager);
            m_vehicleBuffersRegistry = vehicleBuffersRegistry;
            m_cargo = new Dict<ProductProto, ProductBuffer>();
        }

        public void SimUpdate()
        {
            if (Address == null || Address.Trim().Length == 0)
            {
                RevokeAllContracts();
                return;
            }
        }

        private void RevokeAllContracts()
        {
            //List<ContractProto> contracts = m_contractManager.ActiveContracts
            //    .AsEnumerable()
            //    .Where(x => x.Mod.Name == ModDefinition.ModName)
            //    .ToList();
            //
            //foreach (ContractProto item in contracts)
            //{
            //    item.SetAvailability(false);
            //}
        }

        public static void Serialize(MultiplayerTradeDock value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static MultiplayerTradeDock Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out MultiplayerTradeDock value, (Func<BlobReader, Type, MultiplayerTradeDock>)null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        private readonly int SerializerVersion = 0;
        private readonly Dict<ProductProto, ProductBuffer> m_cargo;
        private int m_cargoExportPriority;
        private MultiplayerTradeDockProto m_proto;

        [OnlyForSaveCompatibility(null)]
        [InitAfterLoad(InitPriority.Normal)]
        private void initSelf(int saveVersion)
        {
            //if (saveVersion < 140)
            //{
            //    base.Context.Calendar.NewDay.Add(this, onNewDay);
            //}
        }

        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteInt(SerializerVersion);
            Dict<ProductProto, ProductBuffer>.Serialize(m_cargo, writer);
            writer.WriteInt(m_cargoExportPriority);
            writer.WriteGeneric(m_proto);
            writer.WriteGeneric(m_vehicleBuffersRegistry);
            ReservedOceanAreaState.Serialize(ReservedOceanAreaState, writer);
            writer.WriteGeneric(ReservedOceanProto);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            int version = reader.ReadInt();
            reader.SetField(this, "m_cargo", Dict<ProductProto, ProductBuffer>.Deserialize(reader)
                ?? new Dict<ProductProto, ProductBuffer>());
            m_cargoExportPriority = reader.ReadInt();
            m_proto = reader.ReadGenericAs<MultiplayerTradeDockProto>();
            reader.SetField(this, "m_vehicleBuffersRegistry", reader.ReadGenericAs<IVehicleBuffersRegistry>());
            ReservedOceanAreaState = ReservedOceanAreaState.Deserialize(reader);
            ReservedOceanProto = reader.ReadGenericAs<IProtoWithReservedOcean>();
            reader.RegisterInitAfterLoad(this, "initSelf", InitPriority.Normal);
        }

        public int GetCustomPriority(string id)
        {
            if (id == "CargoExportPrio")
            {
                return m_cargoExportPriority;
            }

            Assert.Fail("Unknown custom priority: " + id);
            return 0;
        }

        public bool IsCustomPriorityVisible(string id)
        {
            if (id == "CargoExportPrio")
            {
                return true;
            }

            return false;
        }

        public void SetCustomPriority(string id, int priority)
        {
            if (GeneralPriorities.AssertAssignableRange(priority))
            {
                if (id == "CargoExportPrio")
                {
                    m_cargoExportPriority = priority;
                }
                else
                {
                    Assert.Fail("Unknown custom priority: " + id);
                }
            }
        }

        public void AddProduct(ProductQuantity demand)
        {
            if (m_cargo.TryGetValue(demand.Product, out var buffer))
            {
                buffer.SetCapacity(buffer.Quantity + demand.Quantity);
                buffer.StoreAsMuchAs(demand);
            }
            else
            {
                m_cargo[demand.Product] = buffer = new ProductBuffer(demand.Quantity, demand.Product);
                buffer.StoreAsMuchAs(demand);
                m_vehicleBuffersRegistry.RegisterOutputBufferAndAssert(this, buffer, new StaticPriorityProvider(BufferStrategy.NoQuantityPreference(1)), true);
            }
        }

        public Quantity GetCargoQuantityOf(ProductProto product)
        {
            if (m_cargo.TryGetValue(product, out ProductBuffer value))
            {
                return value.Quantity;
            }

            return Quantity.Zero;
        }

        public Quantity GetQuantity()
        {
            return m_cargo.Values.Select(v => v.Quantity.Value).Sum().Quantity();
        }

        public ProductQuantity[] GetQuantities()
        {
            return m_cargo.Values.Select(v => v.ProductQuantity).ToArray();
        }
    }
}
