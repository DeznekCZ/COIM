using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Replaces the whole <see cref="Module.ArrayData"/> buffer of a module with a new
	/// Fix32 array.  Used by the bit-field layout editor (Bits: encode / decode) to
	/// persist its per-pin offset/length pairs — the editor is client-side UI, so the
	/// mutation has to travel as a serialized command to stay multiplayer-deterministic,
	/// exactly like <see cref="ModuleSetFix32FieldCmd"/>.  The executor resizes the
	/// module's array to the payload length and copies the values in.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleSetArrayCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly Fix32[] Values;

		public ModuleSetArrayCmd(EntityId controllerId, long moduleId, Fix32[] values)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			Values = values ?? System.Array.Empty<Fix32>();
		}

		public static void Serialize(ModuleSetArrayCmd value, BlobWriter writer)
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
			writer.WriteArray(Values);
		}

		public new static ModuleSetArrayCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleSetArrayCmd obj,
				(Func<BlobReader, Type, ModuleSetArrayCmd>)null,
				(Func<BlobReader, string, ModuleSetArrayCmd>)null,
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
			reader.SetField(this, nameof(Values), reader.ReadArray<Fix32>() ?? System.Array.Empty<Fix32>());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleSetArrayCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleSetArrayCmd)obj).DeserializeData(reader);
	}
}
