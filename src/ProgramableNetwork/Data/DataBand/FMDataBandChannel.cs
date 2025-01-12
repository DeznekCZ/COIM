using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Serialization;
using System.Linq;

namespace ProgramableNetwork
{
    public class FMDataBandChannel : IDataBandChannel
    {
        public int Index { get; set; }
        public Fix32[] Value { get; set; }
        public int ValidIterations { get; set; }
        public Antena Antena { get => m_antena; set { m_antenaId = value?.Id ?? new EntityId(0); m_antena = value; } }

        public FMDataBand OriginalDataBand { get; set; }

        private Antena m_antena;
        private EntityId m_antenaId;

        public static void Serialize(FMDataBandChannel channel, BlobWriter writer)
        {
            writer.WriteByte(/*version*/3);
            writer.WriteInt(channel.Index);
            writer.WriteArray(channel.Value ?? new Fix32[0]);
            writer.WriteInt(channel.ValidIterations);
            writer.WriteInt(channel.m_antenaId.Value);
        }

        public static FMDataBandChannel Deserialize(BlobReader reader)
        {
            var version = reader.ReadByte();
            int index = reader.ReadInt();
            Fix32[] value;
            if (version < 3)
                value = reader.ReadArray<int>().Select(Fix32.FromInt).ToArray();
            else
                value = reader.ReadArray<Fix32>();
            return new FMDataBandChannel()
            {
                Index = index,
                Value = value,
                ValidIterations = reader.ReadInt(),
                m_antenaId = new EntityId(version > 0 ? reader.ReadInt() : 0)
            };
        }

        public void UpdateAntenaReference(FMDataBand self, IEntitiesManager manager)
        {
            OriginalDataBand = self;
            manager.TryGetEntity(m_antenaId, out m_antena);
        }

        public void Update()
        {
            if (Antena?.DataBand is FMDataBand targetDataBand)
            {
                Fix32[] data = OriginalDataBand.Read(Index);
                targetDataBand.Update(Index, data);
            }
        }
    }
}