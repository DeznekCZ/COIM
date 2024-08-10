using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Economy;
using Mafi.Core.Entities;
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

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    internal class MultiplayerTradeVillage : WorldMapVillage
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((MultiplayerTradeVillage)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((MultiplayerTradeVillage)obj).DeserializeData(reader);
        };

        public MultiplayerTradeVillage(EntityId entityId, WorldMapVillageProto prototype, WorldMapLocation location, EntityContext context, ISimLoopEvents simLoopEvents, IAssetTransactionManager assetManager, WorldMapManager worldMapManager, IUpointsManager upointsManager, IInstaBuildManager instaBuildManager, IProductsManager productsManager, TradeDockManager tradeDockManager, SettlementsManager settlementsManager, INotificationsManager notificationsManager, IPropertiesDb propertiesDb, AllImplementationsOf<IVirtualProductQuickTradeHandler> virtualTradeHandlers)
            : base(entityId, prototype, location, context, simLoopEvents, assetManager, worldMapManager, upointsManager, instaBuildManager, productsManager, tradeDockManager, settlementsManager, notificationsManager, propertiesDb, virtualTradeHandlers)
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
    }
}
