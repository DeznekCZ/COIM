using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Shift-stamps a copy of an existing module (source) onto a destination controller
	/// at (row, column).  The source can live on a different controller — the inspector's
	/// "last placed" reference is global across panels — so the cmd carries both the
	/// destination controller id and the source's (controllerId, moduleId).  On apply
	/// the new module is allocated with a fresh id and the source's NumberData,
	/// FieldNumberData, StringData, extension counts, and ArrayData are copied across
	/// so the stamped module reproduces the original's settings exactly.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleShiftAddCmd : InputCommand
	{
		public readonly EntityId DestControllerId;
		public readonly EntityId SourceControllerId;
		public readonly long SourceModuleId;
		public readonly int TargetRow;
		public readonly int TargetColumn;

		/// <summary>
		/// Id of the module the executor stamped on success — 0 if the stamp failed
		/// (overlap / source missing).  Same role as on <see cref="ModulePlaceCmd"/>:
		/// the UI reads this in the OnApplied callback to locate the newly-stamped
		/// module and update its "last placed" reference.
		/// </summary>
		public long CreatedModuleId;

		public ModuleShiftAddCmd(EntityId destControllerId, EntityId sourceControllerId,
			long sourceModuleId, int targetRow, int targetColumn)
		{
			DestControllerId = destControllerId;
			SourceControllerId = sourceControllerId;
			SourceModuleId = sourceModuleId;
			TargetRow = targetRow;
			TargetColumn = targetColumn;
		}

		public static void Serialize(ModuleShiftAddCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(DestControllerId, writer);
			EntityId.Serialize(SourceControllerId, writer);
			writer.WriteLong(SourceModuleId);
			writer.WriteInt(TargetRow);
			writer.WriteInt(TargetColumn);
			writer.WriteLong(CreatedModuleId);
		}

		public new static ModuleShiftAddCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleShiftAddCmd obj,
				(Func<BlobReader, Type, ModuleShiftAddCmd>)null,
				(Func<BlobReader, string, ModuleShiftAddCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(DestControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(SourceControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(SourceModuleId), reader.ReadLong());
			reader.SetField(this, nameof(TargetRow), reader.ReadInt());
			reader.SetField(this, nameof(TargetColumn), reader.ReadInt());
			CreatedModuleId = reader.ReadLong();
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleShiftAddCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleShiftAddCmd)obj).DeserializeData(reader);
	}
}
