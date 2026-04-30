using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets a single named Fix32 property on a <see cref="ProgramableNetwork.Data.DisplayEntity.DisplayEntity"/>.
	/// Used for both colour channels (e.g. <c>colorOn.R</c>) and per-segment toggles
	/// (e.g. <c>A</c>..<c>DP</c> for seven-segment displays).
	/// </summary>
	[ManuallyWrittenSerialization]
	public class DisplayEntitySetPropertyCmd : InputCommand
	{
		public readonly EntityId EntityId;
		public readonly string Name;
		public readonly Fix32 Value;

		public DisplayEntitySetPropertyCmd(EntityId entityId, string name, Fix32 value)
		{
			EntityId = entityId;
			Name = name;
			Value = value;
		}

		public static void Serialize(DisplayEntitySetPropertyCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(EntityId, writer);
			writer.WriteString(Name);
			writer.WriteInt(Value.RawValue);
		}

		public new static DisplayEntitySetPropertyCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out DisplayEntitySetPropertyCmd obj,
				(Func<BlobReader, Type, DisplayEntitySetPropertyCmd>)null,
				(Func<BlobReader, string, DisplayEntitySetPropertyCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(EntityId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(Name), reader.ReadString());
			reader.SetField(this, nameof(Value), Fix32.FromRaw(reader.ReadInt()));
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((DisplayEntitySetPropertyCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((DisplayEntitySetPropertyCmd)obj).DeserializeData(reader);
	}
}
