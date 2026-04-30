using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Wires (or unwires) one input on a module to/from another module's output.
	/// <para>
	/// <see cref="SourceModuleId"/> == 0 (with empty <see cref="SourceOutputId"/>) means
	/// disconnect — the input is removed from <c>InputModules</c>.  Otherwise the input is
	/// set to the given (moduleId, outputId) pair.
	/// </para>
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleSetInputConnectionCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly string InputId;
		public readonly long SourceModuleId;
		public readonly string SourceOutputId;

		public ModuleSetInputConnectionCmd(EntityId controllerId, long moduleId, string inputId,
			long sourceModuleId, string sourceOutputId)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			InputId = inputId;
			SourceModuleId = sourceModuleId;
			SourceOutputId = sourceOutputId ?? string.Empty;
		}

		public bool IsDisconnect => SourceModuleId == 0 && string.IsNullOrEmpty(SourceOutputId);

		public static void Serialize(ModuleSetInputConnectionCmd value, BlobWriter writer)
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
			writer.WriteString(InputId);
			writer.WriteLong(SourceModuleId);
			writer.WriteString(SourceOutputId ?? string.Empty);
		}

		public new static ModuleSetInputConnectionCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleSetInputConnectionCmd obj,
				(Func<BlobReader, Type, ModuleSetInputConnectionCmd>)null,
				(Func<BlobReader, string, ModuleSetInputConnectionCmd>)null,
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
			reader.SetField(this, nameof(InputId), reader.ReadString());
			reader.SetField(this, nameof(SourceModuleId), reader.ReadLong());
			reader.SetField(this, nameof(SourceOutputId), reader.ReadString());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleSetInputConnectionCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleSetInputConnectionCmd)obj).DeserializeData(reader);
	}
}
