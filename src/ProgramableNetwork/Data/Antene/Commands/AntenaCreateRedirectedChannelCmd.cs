using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Adds a new redirected channel to the antena's currently-bound data band.
	/// Replaces the direct <c>Entity.DataBand.CreateChannel()</c> call from the
	/// AntenaInspector "+" button so the action is multiplayer-safe.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class AntenaCreateRedirectedChannelCmd : InputCommand
	{
		public readonly EntityId AntenaId;

		public AntenaCreateRedirectedChannelCmd(EntityId antenaId)
		{
			AntenaId = antenaId;
		}

		public static void Serialize(AntenaCreateRedirectedChannelCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(AntenaId, writer);
		}

		public new static AntenaCreateRedirectedChannelCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out AntenaCreateRedirectedChannelCmd obj,
				(Func<BlobReader, Type, AntenaCreateRedirectedChannelCmd>)null,
				(Func<BlobReader, string, AntenaCreateRedirectedChannelCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(AntenaId), EntityId.Deserialize(reader));
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((AntenaCreateRedirectedChannelCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((AntenaCreateRedirectedChannelCmd)obj).DeserializeData(reader);
	}
}
