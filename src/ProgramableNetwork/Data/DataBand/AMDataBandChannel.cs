using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.World.Entities;
using Mafi.Serialization;
using System.Linq;

namespace ProgramableNetwork
{
    public class AMDataBandChannel : IDataBandChannel
    {
        public int Index { get; set; }
        public Fix32[] Value { get; set; }
        public int ValidIterations { get; set; }
        public Antena Antena { get => m_antena; set { m_antenaId = value?.Id ?? new EntityId(0); m_antena = value; } }
        public WorldMapMine WorldMapMine { get; set; }

        public AMDataBand OriginalDataBand { get; set; }

        private Antena m_antena;
        private EntityId m_antenaId;

        public static void Serialize(AMDataBandChannel channel, BlobWriter writer)
        {
            writer.WriteByte(/*version*/3);
            writer.WriteInt(channel.Index);
            writer.WriteArray(channel.Value ?? new Fix32[0]);
            writer.WriteInt(channel.ValidIterations);
            writer.WriteInt(channel.m_antenaId.Value);
        }

        public static AMDataBandChannel Deserialize(BlobReader reader)
        {
            var version = reader.ReadByte();
            int index = reader.ReadInt();
            Fix32[] value;
            if (version < 3)
                value = reader.ReadArray<int>().Select(Fix32.FromInt).ToArray();
            else
                value = reader.ReadArray<Fix32>();
            return new AMDataBandChannel()
            {
                Index = index,
                Value = value,
                ValidIterations = reader.ReadInt(),
                m_antenaId = new EntityId(version > 0 ? reader.ReadInt() : 0)
            };
        }

        public void UpdateAntenaReference(AMDataBand self, IEntitiesManager manager)
        {
            OriginalDataBand = self;
            manager.TryGetEntity(m_antenaId, out m_antena);
        }

        public void Update()
        {
            if (!(WorldMapMine is null))
            {
                if (Index == 15) // 530 + 17 * 10 => 700
                    Value = new[] { WorldMapMine.Buffer.Quantity.Value.ToFix32() };
                else if (Index == 16) // 530 + 19 * 10 => 710
                    Value = new[] { WorldMapMine.Buffer.Capacity.Value.ToFix32() };
                else if (Index == 16) // 530 + 19 * 10 => 710
                    Value = new[] { WorldMapMine.Buffer.Capacity.Value.ToFix32() };
            }

            if (Antena?.DataBand is AMDataBand targetDataBand)
            {
                Fix32[] data = OriginalDataBand.Read(Index);
                targetDataBand.Update(Index, data);
            }
        }
    }
}