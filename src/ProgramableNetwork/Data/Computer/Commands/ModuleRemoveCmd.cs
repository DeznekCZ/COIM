using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Removes a module from its controller and drops every cable that referenced it.
	/// Inspector dispatches this so the mutation runs on the sim thread, matching the
	/// determinism/MP/replay guarantees of the other field/connection commands.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleRemoveCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;

		public ModuleRemoveCmd(EntityId controllerId, long moduleId)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
		}

		public static void Serialize(ModuleRemoveCmd value, BlobWriter writer)
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
		}

		public new static ModuleRemoveCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleRemoveCmd obj,
				(Func<BlobReader, Type, ModuleRemoveCmd>)null,
				(Func<BlobReader, string, ModuleRemoveCmd>)null,
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
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleRemoveCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleRemoveCmd)obj).DeserializeData(reader);
	}
}
