using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Applies a registered controller template (Python lambda from
	/// <c>ControllerTemplates.CachedTemplates</c>) to a live controller in place —
	/// clears existing modules, runs the template's module-creation lambda, executes
	/// per-module Init, then runs the deferred settings callback.  Identified by the
	/// template's stable id (the same id used by the picker for search/dedupe).
	/// <para>
	/// Blueprint-backed templates ("bp:..." prefix) are deliberately not handled
	/// here — they read from the per-client <c>BlueprintsLibrary</c>, which is local
	/// state, so their <c>Apply</c> stays direct from the UI.
	/// </para>
	/// <para>
	/// TODO MP correctness: the template id assumes every peer has the same Python
	/// templates registered.  If a mod is installed on the originating client only,
	/// the peer's executor will fail to resolve the id and report an error.  A more
	/// robust design serialises the full resulting controller state (modules,
	/// cables, color, description) into the cmd payload — bigger over the wire but
	/// independent of which mods each peer has loaded.
	/// </para>
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ControllerApplyTemplateCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly string TemplateId;

		public ControllerApplyTemplateCmd(EntityId controllerId, string templateId)
		{
			ControllerId = controllerId;
			TemplateId = templateId ?? string.Empty;
		}

		public static void Serialize(ControllerApplyTemplateCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(ControllerId, writer);
			writer.WriteString(TemplateId ?? string.Empty);
		}

		public new static ControllerApplyTemplateCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ControllerApplyTemplateCmd obj,
				(Func<BlobReader, Type, ControllerApplyTemplateCmd>)null,
				(Func<BlobReader, string, ControllerApplyTemplateCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(ControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(TemplateId), reader.ReadString());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ControllerApplyTemplateCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ControllerApplyTemplateCmd)obj).DeserializeData(reader);
	}
}
