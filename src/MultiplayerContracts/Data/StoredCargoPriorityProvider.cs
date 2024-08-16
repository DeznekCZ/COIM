using Mafi.Core.Vehicles;
using Mafi.Serialization;
using System;

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    internal class StoredCargoPriorityProvider : IOutputBufferPriorityProvider
    {
        private readonly MultiplayerTradeDock m_dock;

        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((StoredCargoPriorityProvider)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((StoredCargoPriorityProvider)obj).DeserializeData(reader);
        };

        public StoredCargoPriorityProvider(MultiplayerTradeDock dock)
        {
            this.m_dock = dock;
        }

        public BufferStrategy GetOutputPriority(OutputPriorityRequest request)
        {
            return new BufferStrategy(m_dock.m_cargoExportPriority, request.Buffer.Quantity - request.PendingQuantity);
        }

        public static void Serialize(StoredCargoPriorityProvider value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        protected virtual void SerializeData(BlobWriter writer)
        {
            MultiplayerTradeDock.Serialize(m_dock, writer);
        }

        public static StoredCargoPriorityProvider Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out StoredCargoPriorityProvider obj, (Func<BlobReader, Type, StoredCargoPriorityProvider>)null))
            {
                reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
            }
            return obj;
        }

        protected virtual void DeserializeData(BlobReader reader)
        {
            reader.SetField(this, "m_dock", MultiplayerTradeDock.Deserialize(reader));
        }
    }
}