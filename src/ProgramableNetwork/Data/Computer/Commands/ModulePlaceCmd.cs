using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Places a new module of the given prototype at an absolute (row, column).
	/// The controller's <c>ModuleIdManager</c> allocates a fresh id, the prototype's
	/// <c>ExecuteInit</c> runs, and the resulting module becomes the controller's
	/// "last placed" so the inspector's shift-stamp shortcut can copy from it.
	/// On overlap or out-of-bounds the command is a no-op (no module added).
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModulePlaceCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly Proto.ID ProtoId;
		public readonly int TargetRow;
		public readonly int TargetColumn;

		/// <summary>
		/// Id of the module created by the executor on success — 0 when the placement
		/// failed (overlap / out-of-bounds / missing proto).  Set by the executor
		/// after <c>Controller.TryPlaceModule</c> runs; UI reads this in the OnApplied
		/// callback to locate the new module.  Deterministic across MP peers because
		/// <c>ModuleIdManager</c> allocates the same id on every replica.
		/// </summary>
		public long CreatedModuleId;

		public ModulePlaceCmd(EntityId controllerId, Proto.ID protoId, int targetRow, int targetColumn)
		{
			ControllerId = controllerId;
			ProtoId = protoId;
			TargetRow = targetRow;
			TargetColumn = targetColumn;
		}

		public static void Serialize(ModulePlaceCmd value, BlobWriter writer)
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
			writer.WriteLong(CreatedModuleId);
		}

		public new static ModulePlaceCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModulePlaceCmd obj,
				(Func<BlobReader, Type, ModulePlaceCmd>)null,
				(Func<BlobReader, string, ModulePlaceCmd>)null,
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
			CreatedModuleId = reader.ReadLong();
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModulePlaceCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModulePlaceCmd)obj).DeserializeData(reader);
	}
}
