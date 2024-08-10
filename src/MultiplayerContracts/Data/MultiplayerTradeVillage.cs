using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Economy;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Maintenance;
using Mafi.Core.Notifications;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.PropertiesDb;
using Mafi.Core.Simulation;
using Mafi.Core.Utils;
using Mafi.Core.World;
using Mafi.Core.World.Entities;
using Mafi.Core.World.QuickTrade;
using Mafi.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    internal class MultiplayerTradeVillage : WorldMapMine, IEntityWithSimUpdate
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((MultiplayerTradeVillage)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((MultiplayerTradeVillage)obj).DeserializeData(reader);
        };

        public MultiplayerTradeVillage(EntityId entityId, WorldMapMineProto proto, WorldMapLocation location, EntityContext context, WorldMapManager worldMapManager, IUpointsManager upointsManager, IInstaBuildManager instaBuildManager, IProductsManager productsManager, IEntityMaintenanceProvidersFactory maintenanceProvidersFactory, INotificationsManager notificationsManager)
            
            : base(entityId, proto, location, context, worldMapManager, upointsManager, instaBuildManager, productsManager, maintenanceProvidersFactory, notificationsManager)
        {
        }

        public static void Serialize(MultiplayerTradeVillage value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static new MultiplayerTradeVillage Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out MultiplayerTradeVillage value, (Func<BlobReader, Type, MultiplayerTradeVillage>)null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        private readonly int SerializerVersion = 0;
        private string Address;

        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteInt(SerializerVersion);
            writer.WriteString(Address);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            int version = reader.ReadInt();
            if (version != SerializerVersion)
            {
                // todo handle update
            }
            else
            {
                Address = reader.ReadString();
            }
        }

        private int ticks;
        protected override void SimUpdateInternal()
        {
            base.SimUpdateInternal();

            if (ticks > 300)
            {
                ticks = -1;

                ((ProductBuffer)Buffer).Clear();
                FieldInfo m = typeof(ProductBuffer)
                    .GetField("m_product", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                m.SetValue(this, Context.ProtosDb.All<ProductProto>()
                    .SkipWhile(p => new Random().NextDouble() > 0.5)
                    .Select(p => new ProductBuffer(100.Quantity(), p))
                    .First());
            }

            ticks++;
        }
    }
}
