using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Serialization;
using Mafi.Unity.UiToolkit.Component;
using System;
using System.Linq;

namespace ProgramableNetwork;

public class FMDataBandChannel : IDataBandChannel {
	public const int MaxSize = SignalBufferPool.Size;

	public int Index { get; set; }
	public string Id3 { get; set; }
	/// <summary>
	/// Pooled signal buffer; null when the channel is cold.
	/// Only the first <see cref="Count"/> entries are valid.
	/// </summary>
	public Fix32[] Value { get; private set; }
	public int Count { get; set; }
	public int ValidIterations { get; set; }
	public Antena Antena { get => m_antena; set { m_antenaId = value?.Id ?? new EntityId(0); m_antena = value; } }

	public FMDataBand OriginalDataBand { get; set; }

	private Antena m_antena;
	private EntityId m_antenaId;

	internal void Acquire() {
		if (Value == null) {
			Value = SignalBufferPool.Rent();
		}
	}

	internal void Release() {
		if (Value != null) {
			SignalBufferPool.Return(Value);
			Value = null;
		}
		Count = 0;
	}

	public static void Serialize(FMDataBandChannel channel, BlobWriter writer) {
		// v5: pooled fixed-size buffer + explicit Count. Wire layout is back-compatible
		// with v4 (we still write the array as a length-prefixed Fix32[]); the bump signals
		// the storage-side change and lets future versions add per-channel fields safely.
		writer.WriteByte(/*version*/5);
		writer.WriteInt(channel.Index);
		writer.WriteString(channel.Id3 ?? string.Empty);
		writer.WriteInt(channel.Count);
		Fix32[] slice;
		if (channel.Count > 0 && channel.Value != null) {
			slice = new Fix32[channel.Count];
			Array.Copy(channel.Value, slice, channel.Count);
		} else {
			slice = Array.Empty<Fix32>();
		}
		writer.WriteArray(slice);
		writer.WriteInt(channel.ValidIterations);
		writer.WriteInt(channel.m_antenaId.Value);
	}

	public static FMDataBandChannel Deserialize(BlobReader reader) {
		var version = reader.ReadByte();
		int index = reader.ReadInt();
		string customName = version >= 4 ? reader.ReadString() : string.Empty;
		int explicitCount = version >= 5 ? reader.ReadInt() : -1;
		Fix32[] storedValues;
		if (version < 3) {
			storedValues = reader.ReadArray<int>().Select(Fix32.FromInt).ToArray();
		} else {
			storedValues = reader.ReadArray<Fix32>();
		}
		var channel = new FMDataBandChannel() {
			Index = index,
			Id3 = customName,
			ValidIterations = reader.ReadInt(),
			m_antenaId = new EntityId(version > 0 ? reader.ReadInt() : 0)
		};
		int sourceCount = explicitCount >= 0 ? explicitCount : (storedValues?.Length ?? 0);
		if (sourceCount > 0 && storedValues != null && storedValues.Length > 0) {
			channel.Acquire();
			int copyCount = Math.Min(Math.Min(sourceCount, storedValues.Length), MaxSize);
			Array.Copy(storedValues, channel.Value, copyCount);
			channel.Count = copyCount;
		}
		return channel;
	}

	public void UpdateAntenaReference(FMDataBand self, IEntitiesManager manager) {
		OriginalDataBand = self;
		manager.TryGetEntity(m_antenaId, out m_antena);
	}

	public void Update() {
		if (Antena?.DataBand is FMDataBand targetDataBand) {
			targetDataBand.CopyChannelInto(Index, OriginalDataBand);
			OriginalDataBand.Id3(Index, targetDataBand.GetId3(Index));
		}
	}

	public UiComponent CreateUI(Ui.AntenaInspector antenaInspector, IDataBandChannel channel) {
		return OriginalDataBand.Prototype.Buttons(antenaInspector, channel);
	}

	public void Move(int v) {
		int newIndex = Index + v;
		if (newIndex < 0) {
			Index = OriginalDataBand.Prototype.Channels + newIndex;
		} else if (newIndex >= OriginalDataBand.Prototype.Channels) {
			Index = newIndex - OriginalDataBand.Prototype.Channels;
		} else {
			Index = newIndex;
		}
	}
}
