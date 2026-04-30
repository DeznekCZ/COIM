using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Swaps the data-band proto bound to an antena (e.g. switching the active tab between
	/// FM and AM in <see cref="ProgramableNetwork.Ui.AntenaInspector"/>).  The executor
	/// resolves the proto and instantiates a fresh band via the proto's constructor.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class AntenaSetDataBandCmd : InputCommand
	{
		public readonly EntityId AntenaId;
		public readonly Proto.ID DataBandProtoId;

		public AntenaSetDataBandCmd(EntityId antenaId, Proto.ID dataBandProtoId)
		{
			AntenaId = antenaId;
			DataBandProtoId = dataBandProtoId;
		}

		public static void Serialize(AntenaSetDataBandCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(AntenaId, writer);
			writer.WriteString(DataBandProtoId.Value ?? string.Empty);
		}

		public new static AntenaSetDataBandCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out AntenaSetDataBandCmd obj,
				(Func<BlobReader, Type, AntenaSetDataBandCmd>)null,
				(Func<BlobReader, string, AntenaSetDataBandCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(AntenaId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(DataBandProtoId), new Proto.ID(reader.ReadString()));
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((AntenaSetDataBandCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((AntenaSetDataBandCmd)obj).DeserializeData(reader);
	}
}
