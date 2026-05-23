using System;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Serialization;

namespace NightMod.LampPosts;

/// <summary>
/// Hand-written blob serialization for <see cref="LampPost"/>, kept in this sibling partial file
/// per the project convention. The format mirrors the game's own <c>Beacon</c> entity: the base
/// entity data first, then this entity's own fields. The write and read orders must stay in sync.
/// </summary>
public sealed partial class LampPost {

	public class Versions {
		public const int Initial = 0;
	}

	private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
		(obj, writer) => ((LampPost)obj).SerializeData(writer);

	private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
		(obj, reader) => ((LampPost)obj).DeserializeData(reader);

	public static void Serialize(LampPost value, BlobWriter writer) {
		if (writer.TryStartClassSerialization(value)) {
			writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
		}
	}

	protected override void SerializeData(BlobWriter writer) {
		base.SerializeData(writer);
		writer.WriteInt(Versions.Initial);
		writer.WriteGeneric(Prototype);
		writer.WriteGeneric(m_electricityConsumer);
	}

	public static LampPost Deserialize(BlobReader reader) {
		if (reader.TryStartClassDeserialization(out LampPost obj)) {
			reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
		}
		return obj;
	}

	protected override void DeserializeData(BlobReader reader) {
		base.DeserializeData(reader);
		int version = reader.ReadInt();
		reader.SetField(this, "Prototype", reader.ReadGenericAs<LampPostProto>());
		reader.SetField(this, "m_electricityConsumer", reader.ReadGenericAs<IElectricityConsumer>());
		reader.RegisterInitAfterLoad(this, nameof(initAfterLoad), InitPriority.Low);
	}
}
