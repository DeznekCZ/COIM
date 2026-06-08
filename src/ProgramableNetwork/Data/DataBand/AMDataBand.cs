using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using System.Collections.Generic;
using System.Linq;
using Mafi.Collections.ImmutableCollections;

namespace ProgramableNetwork
{
    public class AMDataBand : IDataBandTyped<AMDataBandChannel>
    {
        private static readonly int SerializerVersion = 1;
        private DataBandProto m_proto;
        private Proto.ID m_protoId;

        public AMDataBand(Antena antena, EntityContext context, DataBandProto prototype)
        {
            Antena = antena;
            Prototype = prototype;
            Context = context;
            m_redirected = new Lyst<AMDataBandChannel>();
            m_active = new Lyst<AMDataBandChannel>();
            for (int i = 0; i < prototype.Channels; i++)
            {
                m_active.Add(new AMDataBandChannel() { Index = i, OriginalDataBand = this });
            }
        }

        private AMDataBand()
        {
        }

        public Antena Antena { get; set; }

        public DataBandProto Prototype
        {
            get
            {
                return m_proto;
            }
            private set
            {
                m_proto = value;
                m_protoId = m_proto.Id;
            }
        }
        public EntityContext Context { get; set; }

        public IEnumerable<IDataBandChannel> Channels => m_redirected?.Cast<IDataBandChannel>() ?? new List<IDataBandChannel>();

        public Computing RequiredComputation => Computing.Zero;

        public Electricity RequiredPower => (Fix32
            .FromRaw( m_redirected
                .Select(c => c.WorldMapMine is null
                           ? Fix32.Zero
                           : c.Distance(c.WorldMapMine) / (Prototype.Distance * Antena.Prototype.DistanceBoost)
                )
                .Sum(f => f.RawValue)
            ) * 200).IntegerPart.Kw();

        private Lyst<AMDataBandChannel> m_redirected;
        private Lyst<AMDataBandChannel> m_active;

        public static void Serialize(AMDataBand dataBand, BlobWriter writer)
        {
            dataBand.SerializeData(writer);
        }

        private void SerializeData(BlobWriter writer)
        {
            writer.WriteString(m_protoId.Value);
            writer.WriteInt(SerializerVersion);
            Lyst<AMDataBandChannel>.Serialize(m_redirected, writer);
            Lyst<AMDataBandChannel>.Serialize(m_active, writer);
        }

        public static AMDataBand Deserialize(BlobReader reader)
        {
            AMDataBand dataBand = new AMDataBand();
            dataBand.DeserializeData(reader);
            return dataBand;
        }

        private void DeserializeData(BlobReader reader)
        {
            m_protoId = new Proto.ID(reader.ReadString());
            int version = reader.ReadInt();
            m_redirected = Lyst<AMDataBandChannel>.Deserialize(reader);
            m_active = Lyst<AMDataBandChannel>.Deserialize(reader);
        }

        public void initContext(Antena antena)
        {
            Antena = antena;
            Log.Info($"Initializing AM BandData");
            var optional = Context.ProtosDb.Get<DataBandProto>(m_protoId);
            if (optional.HasValue)
            {
                Prototype = optional.Value;
                foreach (var channel in m_redirected)
                {
                    channel.UpdateAntenaReference(this, Context.EntitiesManager);
                }
            }
            else
            {
                Log.Error($"Prototype not found: {m_protoId}");
                Prototype = Context.ProtosDb.Get<DataBandProto>(DataBands.DataBand_Unknown).ValueOrThrow("Unknown signal not found");
            }
        }

        public void Update()
        {
            foreach (var item in m_active)
            {
                if (item.ValidIterations-- == 0)
                {
                    // After one second reset signal
                    item.Value = Fix32.Zero;
                }
            }

            foreach (var item in m_redirected)
            {
                item.Update();
            }
        }

        public void Update(int index, Fix32 value)
        {
            m_active[index].Value = value;
            m_active[index].ValidIterations = 60;
        }

        public Fix32 Read(int index, Fix32 def)
        {
            return m_active[index].Value ?? def;
        }

        public void CreateChannel()
        {
            m_redirected.Add(new AMDataBandChannel() { OriginalDataBand = this });
        }

        public void RemoveChannel(IDataBandChannel channel)
        {
            m_redirected.Remove(channel as AMDataBandChannel);
        }

        /// <summary>
        /// Rebuilds <c>m_redirected</c> from a flat (index, sourceId, operation) triplet
        /// array produced by <see cref="Antena.AddToConfig"/>.  Used when the player
        /// game-side clones an AM antena building so the new antena keeps the same
        /// per-channel mine/ship routing instead of starting with an empty redirected
        /// list.  Cross-entity references (mine / ship ids) only resolve when the
        /// source entity still exists in this save — missing entities just leave the
        /// channel unbound, same fallback path the loader uses.
        /// </summary>
        public void RestoreFromConfig(ImmutableArray<int> packed, IEntitiesManager entitiesManager)
        {
            m_redirected.Clear();
            // Each channel = three ints (index, source id, operation enum). A
            // partial triplet at the tail is treated as corrupt input and dropped
            // — same defensive shape as the save-deserialization path.
            for (int i = 0; i + 2 < packed.Length; i += 3)
            {
                var channel = new AMDataBandChannel
                {
                    Index = packed[i],
                    OriginalDataBand = this,
                    Operation = (AMDataBandChannel.AMOperation)packed[i + 2],
                    SourceIdForConfig = packed[i + 1],
                };
                channel.UpdateAntenaReference(this, entitiesManager);
                m_redirected.Add(channel);
            }
        }
    }
}