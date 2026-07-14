using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Replaces a module's prototype with a compatible sibling from the same
	/// <see cref="ModuleSwapGroup"/> (e.g. <c>Sum</c> → <c>Divide</c>), keeping the module's
	/// cables and field values.  The picker is client-side UI, so the mutation travels as a
	/// serialized command to stay multiplayer-deterministic, exactly like
	/// <see cref="ModuleSetArrayCmd"/>.  The executor validates group membership and the
	/// group's swap checks before applying, then prunes any cables bound to pins that the new
	/// prototype no longer has.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleSwapPrototypeCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly string NewProtoId;

		public ModuleSwapPrototypeCmd(EntityId controllerId, long moduleId, string newProtoId)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			NewProtoId = newProtoId ?? string.Empty;
		}

		public static void Serialize(ModuleSwapPrototypeCmd value, BlobWriter writer)
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
			writer.WriteString(NewProtoId);
		}

		public new static ModuleSwapPrototypeCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleSwapPrototypeCmd obj,
				(Func<BlobReader, Type, ModuleSwapPrototypeCmd>)null,
				(Func<BlobReader, string, ModuleSwapPrototypeCmd>)null,
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
			reader.SetField(this, nameof(NewProtoId), reader.ReadString() ?? string.Empty);
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleSwapPrototypeCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleSwapPrototypeCmd)obj).DeserializeData(reader);
	}
}
