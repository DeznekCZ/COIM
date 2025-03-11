using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    public class AMDataBand : IDataBandTyped<AMDataBandChannel>
    {
        private static readonly int SerializerVersion = 1;
        private DataBandProto m_proto;
        private Proto.ID m_protoId;

        public AMDataBand(EntityContext context, DataBandProto prototype)
        {
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

        public void initContext()
        {
            Log.Info($"Initializing FM BandData");
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
    }
}