using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Priorities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Products;
using Mafi.Core.Vehicles;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Core.PathFinding;
using Mafi.Core.Terrain;

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    internal class MultiplayerTradeDock : LayoutEntity, IEntityWithCustomPriority, IEntity, IIsSafeAsHashKey,
        IStaticEntityWithReservedOcean, IStaticEntityWithReservedOceanV2, ILayoutEntity, IStaticEntity,
		IEntityWithPosition, IRenderedEntity, IEntityWithOutputBuffersForUi,
        IAreaSelectableEntity, IEntityWithSimUpdate, IEntityWithSimpleLogisticsControl
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((MultiplayerTradeDock)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((MultiplayerTradeDock)obj).DeserializeData(reader);
        };

        public string Address => m_market;
        public string Authorization => m_markets.ContainsKey(m_market)
            ? m_markets[m_market] : $@"{{""EntityId"":{Id.Value}, ""CreationTime"":0}}";

        
		[DoNotSave(removedInSaveVersion: SaveVersion.V260_UPDATE_4)]
		[Obsolete]
        public ReservedOceanAreaState ReservedOceanAreaState { get; private set; /* for load */ }
		[NewInSaveVersion(SaveVersion.V260_UPDATE_4)]
		public Option<ReservedOceanAreaStateV2> ReservedOceanAreaStateV2 { get; private set; /* for load */ }
		public RectangleTerrainArea2i OceanAreaRequired { get; private set; }
		public RectangleTerrainArea2i OceanAreaDesired { get; private set; }
		public RectangleTerrainArea2i OceanAreaBlocked { get; private set; }
		public ShipHeightClass? ShipHeightClass { get; private set; }
		public HeightTilesF? MinDepthOverride { get; private set; }
		public ThicknessTilesI? MaxHeightOverride { get; private set; }
		public RelTile2i DockDirection { get; private set; }
		public float NarrowRatio { get; private set; }
		public IProtoWithReservedOcean ReservedOceanProto { get; private set; }

        public override bool CanBePaused => false;

        private readonly IVehicleBuffersRegistry m_vehicleBuffersRegistry;
        private StoredCargoPriorityProvider m_storedCargoPrioProvider;

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

        [DoNotSave(0, null)]
        public Dict<string, string> MarketAuthentications => m_markets;
        [DoNotSave(0, null)]
        public string Market { get => m_market ?? "localhost:6442"; set => m_market = value ?? "localhost:6442"; }
        [DoNotSave(0, null)]
        public string MarketName => m_marketNames.ContainsKey(m_market)
            ? m_marketNames[m_market] : $@"Market";
        [DoNotSave(0, null)]
        public Dict<string, string> MarketNames => m_marketNames;

        public LogisticsControl LogisticsInputControl => LogisticsControl.NotAvailable;

        public LogisticsControl LogisticsOutputControl { get; private set; } = LogisticsControl.Enabled;

        public bool IsLogisticsInputDisabled => true;

        public bool IsLogisticsOutputDisabled => LogisticsOutputControl != LogisticsControl.Enabled;
		public IEnumerable<IProductBufferReadOnly> OutputBuffers => m_cargo.Values;
		
        // TODO trade dock extensions
		public Quantity Capacity => Prototype.Capacity;

		public MultiplayerTradeDock(EntityId id, MultiplayerTradeDockProto proto, TileTransform transform, EntityContext context, IVehicleBuffersRegistry vehicleBuffersRegistry)
            : base(id, proto, transform, context)
        {
            Prototype = proto;
            ReservedOceanProto = proto;
            m_vehicleBuffersRegistry = vehicleBuffersRegistry;
            m_storedCargoPrioProvider = new StoredCargoPriorityProvider(this);
            m_cargo = new Dict<ProductProto, ProductBuffer>();

			InitializeOceanAreaData();
        }

		public void InitializeOceanAreaData() {
			m_proto.ComputeOceanAreaData(
				Transform,
				out RectangleTerrainArea2i oceanAreaRequired,
				out RectangleTerrainArea2i oceanAreaDesired,
				out float narrowRatio);

			OceanAreaRequired = oceanAreaRequired;
			OceanAreaDesired = oceanAreaDesired;
			NarrowRatio = narrowRatio;
			DockDirection = new RelTile2f(Transform.TransformMatrix.Transform(new Vector2f(1, 0))).RoundedRelTile2i;

			if (ReservedOceanAreaStateV2.IsNone) {
				ReservedOceanAreaStateV2 = new ReservedOceanAreaStateV2(this,
					IdsCore.Notifications.OceanAccessBlocked,
					IdsCore.Notifications.OceanAccessPartlyBlocked,
					Context.NotificationsManager);
			}
		}

        public void SimUpdate()
        {
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

        private static readonly int SerializerVersion = 4;
        private readonly Dict<ProductProto, ProductBuffer> m_cargo;
        internal int m_cargoExportPriority = 5;
        private MultiplayerTradeDockProto m_proto;

        private Dict<string, string> m_markets = new Dict<string, string>();
        private Dict<string, string> m_marketNames = new Dict<string, string>()
        {
            {  "localhost:6542", "Local market" }
        };
        private string m_market = "localhost:6542";

        [OnlyForSaveCompatibility(null)]
        [InitAfterLoad(InitPriority.Normal)]
        private void initSelf(int saveVersion)
        {
			InitializeOceanAreaData();
        }

        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteInt(SerializerVersion);
            Dict<ProductProto, ProductBuffer>.Serialize(m_cargo, writer);
            writer.WriteInt(m_cargoExportPriority);
            writer.WriteGeneric(m_proto);
            writer.WriteGeneric(m_vehicleBuffersRegistry);
			// Skip 'ReservedOceanAreaState' (deprecated)
			RectangleTerrainArea2i.Serialize(OceanAreaBlocked, writer);
			Option<ReservedOceanAreaStateV2>.Serialize(ReservedOceanAreaStateV2, writer);
            writer.WriteGeneric(ReservedOceanProto);
            writer.WriteBool(!IsLogisticsOutputDisabled);
            writer.WriteGeneric(m_markets);
            writer.WriteGeneric(m_marketNames);
            writer.WriteString(m_market);
            StoredCargoPriorityProvider.Serialize(m_storedCargoPrioProvider, writer);
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
			OceanAreaBlocked = version >= 4
				? RectangleTerrainArea2i.Deserialize(reader)
				: default;
			ReservedOceanAreaStateV2 = version >= 4
				? Option<ReservedOceanAreaStateV2>.Deserialize(reader)
				: default;
			if (version < 4) {
				ReservedOceanAreaState = ReservedOceanAreaState.Deserialize(reader);
			}
            ReservedOceanProto = reader.ReadGenericAs<IProtoWithReservedOcean>();

            if (version > 2)
            {
                LogisticsOutputControl = reader.ReadBool() ? LogisticsControl.Enabled : LogisticsControl.DisabledButVisible;
            }

            m_markets = Dict<string, string>.Deserialize(reader);
            m_marketNames = Dict<string, string>.Deserialize(reader);
            m_market = reader.ReadString();

            if (version > 1) {
				reader.SetField(this, "m_storedCargoPrioProvider", StoredCargoPriorityProvider.Deserialize(reader));
			} else {
				reader.SetField(this, "m_storedCargoPrioProvider", new StoredCargoPriorityProvider(this));
			}

			reader.RegisterInitAfterLoad(this, nameof(initSelf), InitPriority.Normal);
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
                    foreach (var item in m_cargo)
                    {
                        m_vehicleBuffersRegistry.UnregisterOutputBufferAndAssert(item.Value);
                        m_vehicleBuffersRegistry.RegisterOutputBufferAndAssert(this, item.Value, m_storedCargoPrioProvider, true);
                    }
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
                m_vehicleBuffersRegistry.RegisterOutputBufferAndAssert(this, buffer, m_storedCargoPrioProvider, true);
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

        public void SetLogisticsInputDisabled(bool isDisabled)
        {
            // ignore
        }

        public void SetLogisticsOutputDisabled(bool isDisabled)
        {
            LogisticsOutputControl = isDisabled ? LogisticsControl.DisabledButVisible : LogisticsControl.Enabled;
        }
	}
}
