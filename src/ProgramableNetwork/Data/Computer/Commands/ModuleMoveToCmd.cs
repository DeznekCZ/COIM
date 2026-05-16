using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Moves a module to an absolute (row, column) on its controller's layout grid.
	/// The destination is validated against the controller bounds and against
	/// overlap with other modules; on failure the command is a no-op.  Used by
	/// keyboard nudge moves (delta resolved to absolute by the UI), pick-up drops,
	/// and the explicit "move to" path.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleMoveToCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly int TargetRow;
		public readonly int TargetColumn;

		public ModuleMoveToCmd(EntityId controllerId, long moduleId, int targetRow, int targetColumn)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			TargetRow = targetRow;
			TargetColumn = targetColumn;
		}

		public static void Serialize(ModuleMoveToCmd value, BlobWriter writer)
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
			writer.WriteInt(TargetRow);
			writer.WriteInt(TargetColumn);
		}

		public new static ModuleMoveToCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleMoveToCmd obj,
				(Func<BlobReader, Type, ModuleMoveToCmd>)null,
				(Func<BlobReader, string, ModuleMoveToCmd>)null,
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
			reader.SetField(this, nameof(TargetRow), reader.ReadInt());
			reader.SetField(this, nameof(TargetColumn), reader.ReadInt());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleMoveToCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleMoveToCmd)obj).DeserializeData(reader);
	}
}
