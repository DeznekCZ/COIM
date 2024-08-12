using Mafi.Serialization;
using System;

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    public class ContractId : IComparable<ContractId>
    {
        public readonly long CreationTime;
        public readonly long UserIdentifier;

        public ContractId(long creationTime, long userIdentifier)
        {
            this.CreationTime = creationTime;
            this.UserIdentifier = userIdentifier;
        }

        protected void SerializeData(BlobWriter writer)
        {
            writer.WriteLong(CreationTime);
            writer.WriteLong(UserIdentifier);
        }

        protected void DeserializeData(BlobReader reader)
        {
            reader.SetField(this, nameof(CreationTime), reader.ReadLong());
            reader.SetField(this, nameof(UserIdentifier), reader.ReadLong());
        }

        public static void Serialize(ContractId value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static ContractId Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out ContractId value, (Func<BlobReader, Type, ContractId>)null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        public int CompareTo(ContractId other)
        {
            int users = this.UserIdentifier.CompareTo(other.UserIdentifier);
            int times = this.CreationTime.CompareTo(other.CreationTime);

            return users == 0 ? times : users;
        }

        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((ContractId)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((ContractId)obj).DeserializeData(reader);
        };

        public override bool Equals(object obj)
        {
            return obj is ContractId contract
                && this.CreationTime == contract.CreationTime
                && this.UserIdentifier == contract.UserIdentifier;
        }

        public override int GetHashCode()
        {
            return this.CreationTime.GetHashCode() ^ this.UserIdentifier.GetHashCode();
        }
    }
}