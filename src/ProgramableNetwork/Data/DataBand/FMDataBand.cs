using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork
{
	public class FMDataBand : IDataBandTyped<FMDataBandChannel>
	{
		private DataBandProto m_proto;
		private Proto.ID m_protoId;

		public FMDataBand(Antena antena, EntityContext context, DataBandProto prototype)
		{
			Antena = antena;
			Prototype = prototype;
			Context = context;
			m_redirected = new Lyst<FMDataBandChannel>();
			m_active = [];
			// Channel shells are created eagerly, but their signal buffers stay null
			// until first Update — the pool warms them on demand and reclaims them on invalidation.
			for (int i = 0; i < prototype.Channels; i++)
			{
				m_active.Add(new FMDataBandChannel() { Index = i, OriginalDataBand = this });
			}
		}

		public Lyst<FMDataBandChannel> ActiveChannels => m_active;

		private FMDataBand()
		{
		}

		public Antena Antena { get; set; }

		public DataBandProto Prototype
		{
			get
			{
				return m_proto;
			}
			private set
			{
				m_proto = value;
				m_protoId = m_proto.Id;
			}
		}
		public EntityContext Context { get; set; }

		public IEnumerable<IDataBandChannel> Channels => m_redirected?.Cast<IDataBandChannel>() ?? new List<IDataBandChannel>();

		public Computing RequiredComputation => Computing.Zero;

		public Electricity RequiredPower => m_redirected
												.AsEnumerable()
												.Where(c => !(c.Antena is null))
												.Count().Kw() * 50;

		private Lyst<FMDataBandChannel> m_redirected;
		private Lyst<FMDataBandChannel> m_active;

		public static void Serialize(FMDataBand dataBand, BlobWriter writer)
		{
			dataBand.SerializeData(writer);
		}

		private void SerializeData(BlobWriter writer)
		{
			writer.WriteString(m_protoId.Value);
			writer.WriteInt(/* Version */ 0);
			Lyst<FMDataBandChannel>.Serialize(m_redirected, writer);
			Lyst<FMDataBandChannel>.Serialize(m_active, writer);
		}

		public static FMDataBand Deserialize(BlobReader reader)
		{
			FMDataBand dataBand = new FMDataBand();
			dataBand.DeserializeData(reader);
			return dataBand;
		}

		private void DeserializeData(BlobReader reader)
		{
			m_protoId = new Proto.ID(reader.ReadString());
			int version = reader.ReadInt();
			m_redirected = Lyst<FMDataBandChannel>.Deserialize(reader);
			m_active = Lyst<FMDataBandChannel>.Deserialize(reader);
		}

		public void initContext(Antena antena)
		{
			Antena = antena;
			Log.Info($"Initializing FM BandData");

			var optional = Context.ProtosDb.Get<DataBandProto>(m_protoId);
			if (optional.HasValue)
			{
				Prototype = optional.Value;
				foreach (var channel in m_redirected)
				{
					channel.UpdateAntenaReference(this, Context.EntitiesManager);
				}
			}
			else
			{
				Log.Error($"Prototype not found: {m_protoId}");
				Prototype = Context.ProtosDb.Get<DataBandProto>(DataBands.DataBand_Unknown).ValueOrThrow("Unknown signal not found");
			}
		}

		public void Update()
		{
			foreach (var item in m_active)
			{
				if (item.ValidIterations-- == 0)
				{
					// Buffer goes back to the pool; Id3 cleared. Channel shell stays.
					item.Release();
					item.Id3 = string.Empty;
				}
			}

			foreach (var item in m_redirected)
			{
				item.Update();
			}
		}

		public void Update(int index, Fix32[] src, int count, bool logging = false)
		{
			var slot = m_active[index];
			slot.Acquire();
			Array.Copy(src, slot.Value, count);
			slot.Count = count;
			slot.ValidIterations = 60;

			if (logging)
			{
				Log.Info($"[FMDataBand] Written [{index}]: {count}, [{string.Join(",", slot.Value.Take(count))}]");
			}
		}

		public FMDataBandChannel GetChannel(int index)
		{
			return m_active[index];
		}

		/// <summary>
		/// Direct band-to-band channel transfer using a single Array.Copy between the two
		/// pre-allocated pool buffers — no per-tick allocation.
		/// </summary>
		public void CopyChannelInto(int index, FMDataBand dest)
		{
			var s = m_active[index];
			var d = dest.m_active[index];
			if (s.Value == null || s.Count == 0)
			{
				d.Release();
				return;
			}
			d.Acquire();
			Array.Copy(s.Value, d.Value, s.Count);
			d.Count = s.Count;
			d.ValidIterations = s.ValidIterations;
		}

		public void CreateChannel()
		{
			m_redirected.Add(new FMDataBandChannel() { OriginalDataBand = this });
		}

		public void RemoveChannel(IDataBandChannel channel)
		{
			m_redirected.Remove(channel as FMDataBandChannel);
		}

		public void Id3(int channel, string s) {
			m_active[channel].Id3 = s;
		}

		public string GetId3(int channel) {
			return m_active[channel].Id3 ?? string.Empty;
		}
	}
}