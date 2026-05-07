using Mafi;
using Mafi.Serialization;
using System;

namespace ProgramableNetwork
{
	/// <summary>
	/// Single source of truth for module ids.  Holds one monotonic counter that
	/// survives save/load (the `m_nextId` field is serialized).  The simulation
	/// is single-threaded and deterministic, so a plain counter is enough — no
	/// lock, no game-tick offset bookkeeping.
	///
	/// Replaces the legacy <c>DateTime.UtcNow.Ticks</c> generator (non-deterministic
	/// across multiplayer clients) and the per-controller pool that caused cross-
	/// blueprint id collisions because every fresh controller restarted the count
	/// at 1.
	///
	/// <para><b>Access pattern</b></para>
	/// COI's DI mints a fresh instance per game session — via the constructor on a
	/// new game, via <see cref="Deserialize"/> on a loaded save.  Both paths point
	/// <see cref="Instance"/> at the live manager, so callers reach the singleton
	/// through the static field instead of <c>GlobalDependencyResolver.Get&lt;&gt;()</c>.
	/// The DI lookup was brittle on early-init paths (picker previews, module
	/// construction during proto registration) where the resolver isn't ready yet
	/// and would throw — the static field is just a property read.
	///
	/// The static field itself is NOT saved: it's a per-session resolver variable
	/// that gets re-pointed during init by whichever DI path runs (ctor or
	/// Deserialize).  This is what the user requested with "resolver variable
	/// loaded every time in init phase and not saved" — the counter state lives
	/// in <c>m_nextId</c>, the access reference lives in <c>Instance</c>.
	/// </summary>
	[GlobalDependency(RegistrationMode.AsSelf, false, false)]
	[ManuallyWrittenSerialization]
	public class ModuleIdManager
	{
		private long m_nextId;

		public ModuleIdManager()
		{
		}

		/// <summary>
		/// Hands out the next module id and advances the counter.  First allocation
		/// returns 1; the value 0 stays reserved as the "unset" sentinel used by
		/// callers that want to mark an unattached module.
		/// </summary>
		public long Allocate() => ++m_nextId;

		/// <summary>
		/// Ensures the next allocation will return a value strictly greater than
		/// <paramref name="seenId"/>.  Called once per module during load so the
		/// fresh manager (m_nextId = 0 in saves predating this class) can't hand
		/// out an id that collides with one already on disk.  Without this, every
		/// legacy save would re-mint id 1 → ALL modules with legacy id 1 collapse
		/// into a single entity for cable-colour / field-data lookups, which is
		/// what made stock modules in different controllers act like one.
		/// </summary>
		public void EnsureAtLeast(long seenId)
		{
			if (seenId > m_nextId) {
				m_nextId = seenId;
			}
		}

		/// <summary>
		/// Releases the specified module identifier, making it available for reuse by future allocations.
		/// </summary>
		/// <param name="moduleId">The identifier of the module to release.
		/// Must be a valid module identifier previously allocated by the system.</param>
		public void Free(long moduleId) {
			// TODO make a queue of unused ids
			if (moduleId == m_nextId) {
				--m_nextId;
			}
		}

		private void SerializeData(BlobWriter writer)
		{
			writer.WriteInt(/* Version */ 0);
			writer.WriteLong(m_nextId);
		}

		private void DeserializeData(BlobReader reader)
		{
			int version = reader.ReadInt();
			m_nextId = reader.ReadLong();
			// Re-point the resolver at the just-deserialized instance — COI may
			// have constructed a transient default first (lazy or via DI) which
			// would otherwise stay live with m_nextId = 0.  Updating here keeps
			// the static aligned with the save-loaded manager.
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = (object obj, BlobWriter writer) =>
		{
			((ModuleIdManager)obj).SerializeData(writer);
		};

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = (object obj, BlobReader reader) =>
		{
			((ModuleIdManager)obj).DeserializeData(reader);
		};

		public static void Serialize(ModuleIdManager value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value))
			{
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		public static ModuleIdManager Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleIdManager value, null))
			{
				reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
			}
			return value;
		}
	}
}
