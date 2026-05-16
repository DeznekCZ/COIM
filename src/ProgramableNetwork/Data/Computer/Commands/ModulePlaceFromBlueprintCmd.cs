using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Places a new module of the given prototype at (row, column) AND applies a
	/// saved configuration snapshot to it, atomically on the sim thread.  The
	/// snapshot travels as a full <see cref="Module"/> serialised into the cmd
	/// blob — this is critical for MP correctness: the blueprint that produced
	/// the snapshot is local to the originating client's
	/// <see cref="Mafi.Core.Entities.Blueprints.BlueprintsLibrary"/> and would
	/// not exist on peers, so the snapshot has to be carried inline rather than
	/// re-resolved by id on the receiving side.
	/// <para>
	/// The carried module is data-only — it's never wired up as a live module on
	/// any controller; the executor reads its NumberData / FieldNumberData /
	/// StringData / extension counts / ArrayData and copies them onto the freshly
	/// placed module via <see cref="Controller.TryPasteModule"/>.  Connections
	/// (<c>InputModules</c>) are intentionally not applied — see the same rule on
	/// the regular ShiftAdd / Paste paths.
	/// </para>
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModulePlaceFromBlueprintCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly Proto.ID ProtoId;
		public readonly int TargetRow;
		public readonly int TargetColumn;
		// Data-only Module instance carrying the configurable state we want to
		// stamp onto the freshly placed module.  Allocated client-side from the
		// blueprint payload via <see cref="Module.Deserialize"/>, then re-serialised
		// here so every peer reconstructs an identical copy from the cmd blob.
		// Never added to any controller.
		public readonly Module Snapshot;

		/// <summary>
		/// Set by the executor on success — id of the placed module.  0 when
		/// placement failed (overlap / proto missing).  Read by the UI in the
		/// OnApplied callback to locate the new module without scanning by (row,col).
		/// </summary>
		public long CreatedModuleId;

		public ModulePlaceFromBlueprintCmd(EntityId controllerId, Proto.ID protoId,
			int targetRow, int targetColumn, Module snapshot)
		{
			ControllerId = controllerId;
			ProtoId = protoId;
			TargetRow = targetRow;
			TargetColumn = targetColumn;
			Snapshot = snapshot;
		}

		public static void Serialize(ModulePlaceFromBlueprintCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(ControllerId, writer);
			writer.WriteString(ProtoId.Value);
			writer.WriteInt(TargetRow);
			writer.WriteInt(TargetColumn);
			Module.Serialize(Snapshot, writer);
			writer.WriteLong(CreatedModuleId);
		}

		public new static ModulePlaceFromBlueprintCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModulePlaceFromBlueprintCmd obj,
				(Func<BlobReader, Type, ModulePlaceFromBlueprintCmd>)null,
				(Func<BlobReader, string, ModulePlaceFromBlueprintCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(ControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(ProtoId), new Proto.ID(reader.ReadString()));
			reader.SetField(this, nameof(TargetRow), reader.ReadInt());
			reader.SetField(this, nameof(TargetColumn), reader.ReadInt());
			reader.SetField(this, nameof(Snapshot), Module.Deserialize(reader));
			CreatedModuleId = reader.ReadLong();
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModulePlaceFromBlueprintCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModulePlaceFromBlueprintCmd)obj).DeserializeData(reader);
	}
}
