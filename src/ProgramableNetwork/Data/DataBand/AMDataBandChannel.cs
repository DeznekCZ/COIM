using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.World.Entities;
using Mafi.Serialization;
using System;
using System.ComponentModel;
using System.Linq;

namespace ProgramableNetwork
{
    public class AMDataBandChannel : IDataBandChannel
    {
        public int Index { get; set; }
        public Fix32? Value { get; set; }
        public int ValidIterations { get; set; }
        public Antena Antena { get => m_antena; set { m_antenaId = value?.Id ?? new EntityId(0); m_antena = value; } }
        public WorldMapMine WorldMapMine { get => m_mine; set { m_mineId = value?.Id ?? new EntityId(0); m_mine = value; } }

        public AMDataBand OriginalDataBand { get; set; }

        public AMOperation Operation { get => m_operation; set => m_operation = value; }

        private Antena m_antena;
        private EntityId m_antenaId;

        private WorldMapMine m_mine;
        private EntityId m_mineId;

        private AMOperation m_operation;

        public static void Serialize(AMDataBandChannel channel, BlobWriter writer)
        {
            writer.WriteByte(/*version*/4);
            writer.WriteInt(channel.Index);
            writer.WriteBool(channel.Value.HasValue);
            writer.WriteInt(channel.Value?.RawValue ?? 0);
            writer.WriteInt(channel.ValidIterations);
            writer.WriteInt(channel.m_antenaId.Value);
            writer.WriteInt(channel.m_mineId.Value);
            writer.WriteInt((int)channel.m_operation);
        }

        public static AMDataBandChannel Deserialize(BlobReader reader)
        {
            var version = reader.ReadByte();
            int index = reader.ReadInt();
            Fix32? value = null;
            if (version < 3)
            {
                var array = reader.ReadArray<int>().Select(Fix32.FromInt).ToArray();
                if (array.Length > 0)
                    value = array[0];
            }
            else if (version > 3)
            {
                bool exists = reader.ReadBool();
                value = Fix32.FromRaw(reader.ReadInt());
                if (!exists) value = null;
            }
            else
            {
                var array = reader.ReadArray<Fix32>();
                if (array.Length > 0)
                    value = array[0];
            }
            return new AMDataBandChannel()
            {
                Index = index,
                Value = value,
                ValidIterations = reader.ReadInt(),
                m_antenaId = new EntityId(version > 0 ? reader.ReadInt() : 0),
                m_mineId = new EntityId(version > 3 ? reader.ReadInt() : 0),
                m_operation = (AMOperation)(version > 3 ? reader.ReadInt() : 0)
            };
        }

        public void UpdateAntenaReference(AMDataBand self, IEntitiesManager manager)
        {
            OriginalDataBand = self;
            manager.TryGetEntity(m_antenaId, out m_antena);
            manager.TryGetEntity(m_mineId, out m_mine);
        }

        public enum AMOperation
        {
            // ALTERNATE
            [AMName("No action")] None = 0,

            // READS
            [AMName("Read quantity")] ReadQuantity = 1,
            [AMName("Read capacity")] ReadCapacity = 2,
            [AMName("Read usage (0-100%)")] ReadUsage = 3,
            [AMName("Read product")] ReadProduct = 4,
            [AMName("Read pause")] ReadPause = 5,

            // WRITES
            [AMName("Set pause")] WritePause = 24,
            [AMName("Set production")] WriteProduction = 25,
        }

        public void Update()
        {
            if (!(WorldMapMine is null) && (!WorldMapMine.IsPaused || m_operation == AMOperation.WritePause || m_operation == AMOperation.ReadPause))
            {
                switch (m_operation)
                {
                    // READS
                    case AMOperation.ReadQuantity:
                        OriginalDataBand.Update(Index, WorldMapMine.Buffer.Quantity.Value.ToFix32());
                        break;
                    case AMOperation.ReadCapacity:
                        OriginalDataBand.Update(Index, WorldMapMine.Buffer.Capacity.Value.ToFix32());
                        break;
                    case AMOperation.ReadUsage:
                        OriginalDataBand.Update(Index, 100 * WorldMapMine.Buffer.Quantity.Value.ToFix32() / WorldMapMine.Buffer.Capacity.Value.ToFix32());
                        break;
                    case AMOperation.ReadProduct:
                        OriginalDataBand.Update(Index, Fix32.FromRaw(WorldMapMine.Buffer.Product.SlimId.Value));
                        break;
                    case AMOperation.ReadPause:
                        OriginalDataBand.Update(Index, Fix32.FromRaw(WorldMapMine.IsPaused ? 1 : 0));
                        break;

                    // WRITES
                    case AMOperation.WritePause:
                        WorldMapMine.SetPaused(OriginalDataBand.Read(Index, Fix32.Zero) > Fix32.Zero);
                        break;
                    case AMOperation.WriteProduction:
                        WorldMapMine.SetProductionStep(OriginalDataBand.Read(Index, Fix32.Zero).IntegerPart);
                        break;

                    // ALTERNATE
                    default:
                        m_operation = AMOperation.None; // reset unknown value
                        break;
                }
            }
        }
    }

    public class AMNameAttribute : Attribute
    {
        public AMNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}