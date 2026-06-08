using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Restarts a PLC-PY module's compiled runtime: clears the persistent
	/// <c>PlcContext</c> dict so the next tick re-runs the preamble + init
	/// sections, and wipes the last-known run / compile error strings so the
	/// editor and inspector show a clean state.  The compiled AST and the
	/// player's source code are NOT touched — same script, fresh execution.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModulePlcResetCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;

		public ModulePlcResetCmd(EntityId controllerId, long moduleId)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
		}

		public static void Serialize(ModulePlcResetCmd value, BlobWriter writer)
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

		public new static ModulePlcResetCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModulePlcResetCmd obj,
				(Func<BlobReader, Type, ModulePlcResetCmd>)null,
				(Func<BlobReader, string, ModulePlcResetCmd>)null,
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
			(obj, writer) => ((ModulePlcResetCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModulePlcResetCmd)obj).DeserializeData(reader);
	}
}
