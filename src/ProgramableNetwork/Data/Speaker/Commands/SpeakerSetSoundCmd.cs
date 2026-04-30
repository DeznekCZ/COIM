using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets the speaker's bound sound prefab (path).  Replaces the direct
	/// <c>Entity.SetSound</c> call from the inspector dropdown.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class SpeakerSetSoundCmd : InputCommand
	{
		public readonly EntityId EntityId;
		public readonly string SoundPrefab;

		public SpeakerSetSoundCmd(EntityId entityId, string soundPrefab)
		{
			EntityId = entityId;
			SoundPrefab = soundPrefab;
		}

		public static void Serialize(SpeakerSetSoundCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(EntityId, writer);
			writer.WriteString(SoundPrefab ?? string.Empty);
		}

		public new static SpeakerSetSoundCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out SpeakerSetSoundCmd obj,
				(Func<BlobReader, Type, SpeakerSetSoundCmd>)null,
				(Func<BlobReader, string, SpeakerSetSoundCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(EntityId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(SoundPrefab), reader.ReadString());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((SpeakerSetSoundCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((SpeakerSetSoundCmd)obj).DeserializeData(reader);
	}
}
