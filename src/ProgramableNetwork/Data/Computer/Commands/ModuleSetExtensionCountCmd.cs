using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>Which extension dimension a <see cref="ModuleSetExtensionCountCmd"/> targets.</summary>
	public enum ExtensionSide : byte
	{
		Input = 0,
		Output = 1,
		Display = 2,
	}

	/// <summary>
	/// Sets the per-instance extension count for one side (input pins, output pins, or
	/// display width) of a module.  The executor clamps to <c>[0, MaxXxxExtensions]</c>
	/// on the live prototype and prunes any cable bound to a pin that just disappeared,
	/// keeping the controller in a consistent state.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleSetExtensionCountCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly ExtensionSide Side;
		public readonly int NewCount;

		public ModuleSetExtensionCountCmd(EntityId controllerId, long moduleId, ExtensionSide side, int newCount)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			Side = side;
			NewCount = newCount;
		}

		public static void Serialize(ModuleSetExtensionCountCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(ControllerId, writer);
			writer.WriteLong(ModuleId);
			writer.WriteByte((byte)Side);
			writer.WriteInt(NewCount);
		}

		public new static ModuleSetExtensionCountCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleSetExtensionCountCmd obj,
				(Func<BlobReader, Type, ModuleSetExtensionCountCmd>)null,
				(Func<BlobReader, string, ModuleSetExtensionCountCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(ControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(ModuleId), reader.ReadLong());
			reader.SetField(this, nameof(Side), (ExtensionSide)reader.ReadByte());
			reader.SetField(this, nameof(NewCount), reader.ReadInt());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleSetExtensionCountCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleSetExtensionCountCmd)obj).DeserializeData(reader);
	}
}
