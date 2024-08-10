using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Entities;
using Mafi.Core.Vehicles;
using Mafi.Core.World.Contracts;
using Mafi.Core.World.Loans;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    internal class MultiplayerTradeDock : TradeDock, IEntityWithSimUpdate
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
        private readonly ContractsManager m_contractManager;

        public MultiplayerTradeDock(EntityId id, MultiplayerTradeDockProto proto, TileTransform transform, EntityContext context, ContractsManager contractManager, IVehicleBuffersRegistry vehicleBuffersRegistry)
            : base(id, proto, transform, context, null, vehicleBuffersRegistry)
        {
            this.m_contractManager = contractManager;
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

        public static new MultiplayerTradeDock Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out MultiplayerTradeDock value, (Func<BlobReader, Type, MultiplayerTradeDock>)null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        private readonly int SerializerVersion = 0;

        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteInt(SerializerVersion);
            writer.WriteString(Address ?? "");
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
                try // TODO remove before deploy
                {
                    Address = reader.ReadString();
                }
                catch {}
            }
        }
    }
}
