using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets the "on" colour of a colourised light in one go.  The executor derives the
	/// "off" colour from the same value (each channel clamped at 100), matching what the
	/// inspector used to do with six individual <see cref="DisplayEntitySetPropertyCmd"/>
	/// dispatches.  One command means one tick boundary instead of six, which keeps the
	/// six channels strictly atomic on multiplayer hosts/clients.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class DisplayEntitySetLightColorCmd : InputCommand
	{
		public readonly EntityId EntityId;
		public readonly ColorRgba Color;

		public DisplayEntitySetLightColorCmd(EntityId entityId, ColorRgba color)
		{
			EntityId = entityId;
			Color = color;
		}

		public static void Serialize(DisplayEntitySetLightColorCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(EntityId, writer);
			ColorRgba.Serialize(Color, writer);
		}

		public new static DisplayEntitySetLightColorCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out DisplayEntitySetLightColorCmd obj,
				(Func<BlobReader, Type, DisplayEntitySetLightColorCmd>)null,
				(Func<BlobReader, string, DisplayEntitySetLightColorCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(EntityId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(Color), ColorRgba.Deserialize(reader));
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((DisplayEntitySetLightColorCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((DisplayEntitySetLightColorCmd)obj).DeserializeData(reader);
	}
}
