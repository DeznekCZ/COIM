using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Copies configurable state (NumberData / FieldNumberData / StringData /
	/// input + output + display extension counts / ArrayData) from a source
	/// module onto a destination module that already exists on a controller.
	/// Replaces the inspector's "paste from last-created" handler which
	/// previously mutated the destination directly from the UI thread.
	/// <para>
	/// The source module reference (controller id + module id) is safe to send
	/// across MP peers because the inspector's <c>m_lastCreated</c> only ever
	/// points at a module that was itself placed via a command, so every peer
	/// has the same module to read from.  Connections (<c>InputModules</c>)
	/// are intentionally NOT copied — the source's cable endpoints don't
	/// generally make sense on the destination.
	/// </para>
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModulePasteCmd : InputCommand
	{
		public readonly EntityId DestControllerId;
		public readonly long DestModuleId;
		public readonly EntityId SourceControllerId;
		public readonly long SourceModuleId;

		public ModulePasteCmd(EntityId destControllerId, long destModuleId,
			EntityId sourceControllerId, long sourceModuleId)
		{
			DestControllerId = destControllerId;
			DestModuleId = destModuleId;
			SourceControllerId = sourceControllerId;
			SourceModuleId = sourceModuleId;
		}

		public static void Serialize(ModulePasteCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(DestControllerId, writer);
			writer.WriteLong(DestModuleId);
			EntityId.Serialize(SourceControllerId, writer);
			writer.WriteLong(SourceModuleId);
		}

		public new static ModulePasteCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModulePasteCmd obj,
				(Func<BlobReader, Type, ModulePasteCmd>)null,
				(Func<BlobReader, string, ModulePasteCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(DestControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(DestModuleId), reader.ReadLong());
			reader.SetField(this, nameof(SourceControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(SourceModuleId), reader.ReadLong());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModulePasteCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModulePasteCmd)obj).DeserializeData(reader);
	}
}
