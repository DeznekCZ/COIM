using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets the controller's display color. Replaces the direct Entity.SetColor call so the change
	/// flows through the input scheduler and stays multiplayer-correct.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ControllerSetColorCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly ColorRgba Color;

		public ControllerSetColorCmd(EntityId controllerId, ColorRgba color)
		{
			ControllerId = controllerId;
			Color = color;
		}

		public static void Serialize(ControllerSetColorCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(ControllerId, writer);
			ColorRgba.Serialize(Color, writer);
		}

		public new static ControllerSetColorCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ControllerSetColorCmd obj,
				(Func<BlobReader, Type, ControllerSetColorCmd>)null,
				(Func<BlobReader, string, ControllerSetColorCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(ControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(Color), ColorRgba.Deserialize(reader));
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ControllerSetColorCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ControllerSetColorCmd)obj).DeserializeData(reader);
	}
}
