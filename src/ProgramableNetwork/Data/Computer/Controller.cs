using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Maintenance;
using Mafi.Core.Notifications;
using Mafi.Core.Population;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.Trains;
using Mafi.Core.Vehicles;
using Mafi.Localization;
using Mafi.Serialization;
using ProgramableNetwork.Data.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProgramableNetwork
{
	[ManuallyWrittenSerialization]
	public class Controller : LayoutEntityBase, IAreaSelectableEntity, IEntityWithCloneableConfig, IEntityWithSimUpdate,
		IUnityConsumingEntity, IComputingConsumingEntity, IElectricityConsumingEntity, IMaintainedEntity, IObjectWithCustomTitle
	{
		// Serialization version where the per-cell layout grid (Controller.Rows) was
		// dropped and each Module started carrying its own (Row, Column). Used by both
		// Controller and Module deserialization for version-gated reads.
		public const int MODULE_LAYOUT_INFO = 4;

		// Serialization version where Module's optional containers (the five Number/
		// String dicts, InputModules, and the new Fix32[] ArrayData scratch buffer)
		// are gated behind a single byte bitmask so empty containers cost nothing
		// on disk.  Older saves read each dict unconditionally; v5+ writes only the
		// populated ones.  ArrayData was introduced in this same version — pre-v5
		// modules load with an empty array.
		public const int MODULE_COMPACT_DATA = 5;

		// Controller serialization version where the per-instance CustomDescription
		// field was added.  Earlier saves load with no description set; a fresh
		// CustomDescription = None is the safe default.
		public const int CONTROLLER_DESCRIPTION = 5;

		// Serialization version where the PLC (player-authored Python) module
		// landed.  The DataFlags byte gained a CodeMetadata bit (1 << 7) so PLC
		// instances can persist their cached lexer-node count alongside the
		// existing dicts; pre-v6 modules read with that bit absent and a
		// node count of 0 (re-tokenized on first execute).  Forward-compatible
		// with non-PLC modules — they simply never set the bit.
		public const int MODULE_PYTHON_CODE = 6;

		// Serialization version where Module gained per-instance pin extension
		// counts (InputExtensionCount / OutputExtensionCount) — extra input and/or
		// output pins added by the player on the right side of an extensible
		// prototype.  Pre-v7 saves load with both counts at 0 (no extensions),
		// matching the original behavior.
		public const int MODULE_EXTENSIONS = 7;

		// Display extension count was added a step later at the same conceptual
		// "extensions" feature but a separate version bump is required because
		// in-progress dev saves at v7 were written with only the two pin counts
		// (Input + Output) — reading a third int there walks past the end of the
		// module's data and corrupts the byte stream.  v8+ writes all three; v7
		// loads just the two and leaves DisplayExtensionCount at 0 so the player
		// can grow the display from the inspector after load.
		public const int MODULE_DISPLAY_EXTENSIONS = 8;

		// Controller serialization version where module ids switched from a
		// time-based source (DateTime.UtcNow.Ticks + Thread.Sleep(1)) to a
		// per-controller pool counter persisted on the controller itself.
		// Pre-v6 saves don't carry the counter; on load, initContexts seeds
		// m_nextModuleId from <c>max(existing module ids)</c> so subsequent
		// allocations stay unique within the controller.  Existing modules
		// keep their original time-based ids — the migration is additive.
		public const int CONTROLLER_MODULE_ID_POOL = 6;

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate(object obj, BlobWriter writer)
		{
			((Controller) obj).SerializeData(writer);
		};
		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
		{
			((Controller) obj).DeserializeData(reader);
		};

		public Option<string> CustomTitle { get; set; }

		// Legacy field kept on disk so old saves can still round-trip — for v6+ saves
		// it gets read/written but is no longer consulted at allocation time.  IDs
		// now come from <see cref="ModuleIdManager"/> which holds a single monotonic
		// counter and is serialized with the save; access is via the static
		// <see cref="ModuleIdManager.Instance"/> resolver variable rather than
		// GlobalDependencyResolver.Get<>().  The previous per-controller pool was
		// the source of cable-colour and entity-field collisions: every fresh
		// controller restarted at 1, so two pasted modules from different
		// blueprints could end up with identical ids on a single controller and
		// share their colour-palette / field-data lookups.
		private long m_nextModuleId;

		/// <summary>
		/// Hands out a fresh module id from the serialized global pool so values
		/// never collide across controllers, blueprint pastes, or template
		/// applications.  The legacy per-controller pool name is preserved to keep
		/// callers (BlueprintControllerTemplateEntry, etc.) source-compatible.
		/// </summary>
		public long AllocateModuleId() => Resolver.Resolve<ModuleIdManager>().Allocate();

		// Player-writable free-form description.  Auto-populated when a template/
		// blueprint is applied via the picker (set to the template's description),
		// editable in the inspector after that.  The on-screen rendering ALWAYS
		// has the live module list appended via <see cref="GetFullDescription"/> —
		// only the user-supplied prefix is persisted here.
		public Option<string> CustomDescription { get; set; }

		/// <summary>
		/// Returns the user-supplied description (if any) followed by an auto-generated
		/// list of every module currently on the controller.  The module-list tail is
		/// always present so a player browsing the inspector can see what's inside even
		/// when no description was authored.  Computed on demand — module list reflects
		/// the live state.
		/// </summary>
		public string GetFullDescription()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (CustomDescription.HasValue && !string.IsNullOrEmpty(CustomDescription.Value))
			{
				sb.Append(CustomDescription.Value);
				sb.Append("\n\n");
			}
			sb.Append("Modules:");
			if (Modules == null || Modules.Count == 0)
			{
				sb.Append(" (none)");
			}
			else
			{
				foreach (Module m in Modules)
				{
					if (m?.Prototype == null) {
						continue;
					}
					sb.Append("\n  ");
					sb.Append(m.Prototype.Symbol);
					sb.Append("  ");
					sb.Append(m.Prototype.Strings.Name.TranslatedString);
				}
			}
			return sb.ToString();
		}

		public Controller(EntityId id, ControllerProto proto, TileTransform transform, EntityContext context,
			IEntityMaintenanceProvidersFactory maintenanceProvidersFactory,
			DependencyResolver resolver)
			: base(id, proto, transform, context)
		{
			// Mafi's [InitAfterLoad] hook only fires on deserialize, so brand-new
			// controllers (just placed by the player) don't go through initContexts
			// and would have Resolver = null.  Taking it as a ctor arg fixes that —
			// the entity factory passes the live DependencyResolver so module-add,
			// blueprint-save, and every other Resolver.Resolve<>() call site work
			// from the moment the controller exists in the world.
			Resolver = resolver;
			Prototype = proto.BasedOn ?? proto;
			ErrorMessage = "";
			Color = proto.DefaultColor;
			m_unityConsumer = Context.UnityConsumerFactory.CreateConsumer(this);
			m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_computingConsumer = Context.ComputingConsumerFactory.CreateConsumer(this);
			m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);
			m_notificationInfoManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.InfoNotification);
			m_notificationWarningManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.WarningNotification);
			m_notificationErrorManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.ErrorNotification);
			Modules = new Lyst<Module>();

			Action initSettings = proto.InitModules(this);
			foreach (Module module in Modules)
			{
				module.Prototype.ExecuteInit(module);
			}
			initSettings();

			Log.Info($"Created with {Prototype.Rows} rows, {Prototype.Columns} columns");
		}

		[DoNotSave(0, null)]
		private ControllerProto m_proto;
		[DoNotSave(0, null)]
		private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;

		[DoNotSave(0, null)]
		public new ControllerProto Prototype
		{
			get
			{
				return m_proto;
			}
			protected set
			{
				m_proto = value;
				m_protoId = m_proto.Id;
				base.Prototype = value;
			}
		}

		[DoNotSave(0, null)]
		public override bool CanBePaused => true;
		[DoNotSave(0, null)]
		public bool CanWorkOvertime => true;

		public void AddToConfig(EntityConfigData data)
		{
			// TODO copy modules, name, entity connections, ...
			data.Set<StaticEntityProto.ID>("controller_proto", m_protoId, (str, blob) => blob.WriteString(str.Value));
			data.SetArray<Module>("controller_modules", Modules.ToImmutableArray(), Module.Serialize);
			// Module positions are carried on the modules themselves since MODULE_LAYOUT_INFO,
			// so no separate "controller_rows" entry is needed.
			data.SetInt("controller_speed", DelayBetweenTicks);
			data.SetInt("color", (int)Color.Rgba);
			// Persist the user-supplied description across blueprints/clones.  The
			// auto-appended module list is recomputed on display from live state, so
			// we only write the user's prefix here.
			if (CustomDescription.HasValue) {
				data.SetString("controller_description", CustomDescription.Value);
			}
		}

		public void ApplyConfig(EntityConfigData data)
		{
			// TODO copy modules, name, entity connections ...
			// TODO validate connection and rotate connection when possible
			var newProto = data.Get("controller_proto", (blob) => new StaticEntityProto.ID(blob.ReadString()));
			if (newProto != m_protoId) {
				// TODO play sound
				return;
			}

			var newModules = data.GetArray("controller_modules", Module.Deserialize);
			if (newModules != null)
			{
				this.Modules.Clear();
				this.Modules.AddRange(newModules?.AsEnumerable());
				foreach (Module module in this.Modules)
				{
					module.Context = Context;
					module.Controller = this;
					module.initContexts(-1);
					foreach (IField field in module.Prototype.Fields)
					{
						field.Validate(module);
					}
				}
			}
			// Legacy clones may still have "controller_rows"; back-fill module positions if so.
			ImmutableArray<Lyst<ModulePlacement>>? newLocation =
				data.GetArray("controller_rows", Lyst<ModulePlacement>.Deserialize);
			if (newLocation.HasValue)
			{
				MigrateLegacyRowsIntoModules(newLocation.Value.AsEnumerable());
			}
			var newSpeed = data.GetInt("controller_speed");
			if (newSpeed != null)
			{
				this.DelayBetweenTicks = newSpeed.Value;
			}

			Option<string> savedDescription = data.GetString("controller_description");
			if (savedDescription.HasValue)
			{
				CustomDescription = savedDescription;
			}

			int? color = data.GetInt("color");
			if (color != null)
			{
				this.Color = (uint)color.Value;
			}
		}

		private void MigrateLegacyRowsIntoModules(IEnumerable<Lyst<ModulePlacement>> legacyRows)
		{
			var moduleById = new Dictionary<long, Module>();
			foreach (var m in Modules)
			{
				moduleById[m.Id] = m;
			}
			int rowIdx = 0;
			foreach (var row in legacyRows)
			{
				for (int col = 0; col < row.Count; col++)
				{
					var p = row[col];
					if (p.Placement && p.ModuleId != 0 && moduleById.TryGetValue(p.ModuleId, out var m))
					{
						m.Row = rowIdx;
						m.Column = col;
					}
				}
				rowIdx++;
			}
		}

		public static void Serialize(Controller value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value))
			{
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		public static Controller Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out Controller value, (Func<BlobReader, Type, Controller>)null))
			{
				reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
			}
			return value;
		}

		[InitAfterLoad(InitPriority.Normal)]
		[OnlyForSaveCompatibility(null)]
		private void initContexts(int saveVersion, DependencyResolver resolver)
		{
			Resolver = resolver;
			Log.Info($"Initialize context after load");

			Prototype = Context.ProtosDb.Get<ControllerProto>(m_protoId).ValueOrThrow("Invalid controller proto: " + m_protoId);
			m_electricConsumer = m_electricConsumer ?? Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_computingConsumer = m_computingConsumer ?? Context.ComputingConsumerFactory.CreateConsumer(this);

			m_notificationInfoManager = WithId(ControllerNotification.InfoNotification, m_notificationInfoManager);
			m_notificationWarningManager = WithId(ControllerNotification.WarningNotification, m_notificationWarningManager);
			m_notificationErrorManager = WithId(ControllerNotification.ErrorNotification, m_notificationErrorManager);

			if (Modules == null)
			{
				Modules = new Lyst<Module>();
			}
			else
			{
				// Pre-v6 saves had no persisted module-id pool counter.  Seed the legacy
				// per-controller field from the highest existing module id (kept on
				// disk for round-trip compatibility).
				if (m_nextModuleId == 0)
				{
					long maxId = 0;
					foreach (var m in Modules)
					{
						if (m.Id > maxId) {
							maxId = m.Id;
						}
					}
					m_nextModuleId = maxId;
				}

				// Seed the live ModuleIdManager from every module loaded on this
				// controller.  Saves predating the manager have m_nextId = 0 in
				// its blob, so the very first Allocate would otherwise return 1
				// — colliding with every legacy module that already has id 1 from
				// the pre-manager per-controller pool.  Instance is lazy-initialized
				// so it's guaranteed non-null, and reaching the manager through it
				// avoids the GlobalDependencyResolver.Get<>() failure mode that
				// previously aborted the whole load and left modules with null
				// Prototype.
				ModuleIdManager idManager = resolver.Resolve<ModuleIdManager>();
				foreach (var m in Modules)
				{
					idManager.EnsureAtLeast(m.Id);
				}

				// Resolve prototypes for every module.  Phantom modules (proto removed and
				// no Deprecation replacement) are KEPT as visible tombstones — the player
				// sees a "!!" cell with an "Original prototype no longer exists" tooltip
				// and can decide whether to delete it manually.  We just skip field
				// validation for them since Phantom carries no fields.
				foreach (var m in Modules)
				{
					m.Controller = this;
					m.Context = Context;
					m.initContexts(saveVersion);

					if (m.Prototype == null) {
						Log.Warning($"Module {m.Id} with null prototype found in controller {Id}");
						continue;
					}
					if (m.Prototype == ModuleProto.Phantom) {
						Log.Warning($"Module {m.Id} resolved to Phantom in controller {Id}; original prototype no longer exists");
						continue;
					}

					foreach (IField field in m.Prototype.Fields)
					{
						field.Validate(m);
					}
				}

				// Reattach connections whose endpoints renamed pins in a Deprecation
				// entry, then drop the ones that are still unresolvable.  Without this,
				// cable rendering tries to look up pin protos on Phantom (no Inputs/
				// Outputs) or hits stale references when a mod author renamed/removed a
				// pin in a new version.  Death conditions:
				//   - source module no longer exists,
				//   - source module exists but its prototype has no such output id (after remap),
				//   - this module's prototype has no such input id,
				//   - either side is a Phantom (no pins by definition).
				var moduleById = new Dictionary<long, Module>();
				foreach (var m in Modules) { moduleById[m.Id] = m; }

				// Cache the OutputIdMap for each module that was migrated and carries one,
				// so we don't re-resolve the migration entry per consumer connection.
				var outputRemapBySource = new Dictionary<long, IReadOnlyDictionary<string, string>>();
				foreach (var m in Modules)
				{
					var mig = Deprecation.GetMigration(new ModuleProto.ID(m.OriginalProtoId));
					if (mig.HasValue && mig.Value.AppliesAt(m.LoadedVersion) && mig.Value.OutputIdMap != null)
					{
						outputRemapBySource[m.Id] = mig.Value.OutputIdMap;
					}
				}

				foreach (var m in Modules)
				{
					if (m.Prototype == null) {
						continue;
					}
					foreach (var kv in m.InputModules.ToList())
					{
						if (!moduleById.TryGetValue(kv.Value.ModuleId, out var src))
						{
							m.InputModules.Remove(kv.Key);
							continue;
						}
						if (src.Prototype == null || src.Prototype == ModuleProto.Phantom)
						{
							m.InputModules.Remove(kv.Key);
							continue;
						}
						if (m.Prototype == ModuleProto.Phantom)
						{
							m.InputModules.Remove(kv.Key);
							continue;
						}

						// Apply the source's output rename before validating, so cables
						// drawn from a renamed output pin reattach to the new id instead
						// of being dropped as orphans.  Only the OutputId on the connector
						// changes; ModuleId stays the same.
						var connector = kv.Value;
						if (outputRemapBySource.TryGetValue(src.Id, out var outMap)
							&& outMap.TryGetValue(connector.OutputId, out string newOutputId)
							&& !string.Equals(connector.OutputId, newOutputId, StringComparison.Ordinal))
						{
							connector = new ModuleConnector(connector.ModuleId, newOutputId);
							m.InputModules[kv.Key] = connector;
							Log.Info($"Module {m.Id}: output pin remapped on connection '{kv.Key}' — source {src.Id} '{kv.Value.OutputId}' -> '{newOutputId}' via Deprecation");
						}

						if (!src.HasOutput(connector.OutputId))
						{
							Log.Warning($"Module {m.Id}: dropping connection for input '{kv.Key}' — source module {src.Id} no longer has output '{connector.OutputId}'");
							m.InputModules.Remove(kv.Key);
							continue;
						}
						if (!m.HasInput(kv.Key))
						{
							Log.Warning($"Module {m.Id}: dropping connection — input '{kv.Key}' no longer exists on this module's prototype");
							m.InputModules.Remove(kv.Key);
						}
					}
				}
			}

			if (m_legacyRows != null)
			{
				MigrateLegacyRowsIntoModules(m_legacyRows);
				m_legacyRows = null;
			}

			if (Color == ColorRgba.Empty)
			{
				Color = m_proto.DefaultColor;
			}

			m_productMaintenanceT1 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT1);
			m_productMaintenanceT2 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT2);
			m_productMaintenanceT3 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT3);
		}

		private EntityNotificator WithId(EntityNotificationProto.ID newNotification, EntityNotificator notification)
		{
			if (!m_reninitNotification)
			{
				return Context.NotificationsManager.CreateNotificatorFor(newNotification);
			}

			PropertyInfo field = typeof(EntityNotificator).GetProperty("NotificationId");
			object v = Context.NotificationsManager.CreateNotificatorFor(newNotification);
			field.SetValue(v, notification.NotificationId);
			return (EntityNotificator)v;
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			writer.WriteString(m_protoId.Value);
			writer.WriteInt(/*Version*/ CONTROLLER_MODULE_ID_POOL);

			writer.WriteString(ErrorMessage ?? "");
			Option<string>.Serialize(CustomTitle, writer);
			ColorRgba.Serialize(Color, writer);

			writer.WriteInt(GeneralPriority);
			writer.WriteGeneric(m_maintenanceConsumer);
			writer.WriteGeneric(m_electricConsumer);
			writer.WriteGeneric(m_computingConsumer);

			writer.WriteUInt(m_notificationInfoManager.NotificationId.Value);
			writer.WriteUInt(m_notificationWarningManager.NotificationId.Value);
			writer.WriteUInt(m_notificationErrorManager.NotificationId.Value);

			writer.WriteInt(m_clockSpeed);
			writer.WriteInt(m_clock);

			Lyst<Module>.Serialize(Modules, writer);
			// Layout grid (Rows) was dropped at MODULE_LAYOUT_INFO; positions live on each Module now.

			// CONTROLLER_DESCRIPTION (v5+): player-writable description.  Empty Option
			// is the safe default for old saves loaded back through the v<5 branch.
			Option<string>.Serialize(CustomDescription, writer);

			// CONTROLLER_MODULE_ID_POOL (v6+): per-controller monotonic module-id
			// counter.  Old saves load with 0 here and the post-load init seeds it
			// from max(existing module ids) before any AllocateModuleId call.
			writer.WriteLong(m_nextModuleId);
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			m_protoId = new Mafi.Core.Entities.Static.StaticEntityProto.ID(reader.ReadString());
			int version = reader.ReadInt();

			CurrentInstruction = 0;

			ErrorMessage = reader.ReadString();
			CustomTitle = Option<string>.Deserialize(reader);
			if (version < 3)
			{
				Color = ColorRgba.Empty;
			}
			else
			{
				Color = ColorRgba.Deserialize(reader);
			}

			GeneralPriority = reader.ReadInt();
			m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();

			if (version >= 1)
			{
				m_electricConsumer = reader.ReadGenericAs<IElectricityConsumer>();
				m_computingConsumer = reader.ReadGenericAs<IComputingConsumer>();

				m_reninitNotification = true;

				object v = m_notificationInfoManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationInfoManager = (EntityNotificator)v;

				v = m_notificationWarningManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationWarningManager = (EntityNotificator)v;

				v = m_notificationErrorManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationErrorManager = (EntityNotificator)v;
			}

			if (version >= 2)
			{
				m_clockSpeed = reader.ReadInt();
				m_clock   = reader.ReadInt();
			}
			else
			{
				m_clockSpeed = 0;
				m_clock   = 0;
			}

			Modules = Lyst<Module>.Deserialize(reader);
			if (version < MODULE_LAYOUT_INFO)
			{
				// Legacy save: positions live in the controller's grid. Stash and back-fill
				// into modules in initContexts (after prototypes are resolved).
				m_legacyRows = Lyst<Lyst<ModulePlacement>>.Deserialize(reader);
			}

			// CONTROLLER_DESCRIPTION (v5+): player-writable description string.
			// Pre-v5 saves had no field — leave CustomDescription = None.
			if (version >= CONTROLLER_DESCRIPTION)
			{
				CustomDescription = Option<string>.Deserialize(reader);
			}

			// CONTROLLER_MODULE_ID_POOL (v6+): persisted module-id pool counter.
			// Pre-v6 saves don't carry it; initContexts seeds it from max(existing
			// module ids) so the next AllocateModuleId can't collide with legacy
			// time-based ids that came along for the ride.
			if (version >= CONTROLLER_MODULE_ID_POOL)
			{
				m_nextModuleId = reader.ReadLong();
			}
			else
			{
				m_nextModuleId = 0;
			}

			Log.Info($"Deserialized with {Modules.Count} modules" +
				(m_legacyRows != null ? $" + {m_legacyRows.Count} legacy rows (will migrate)" : ""));
			reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
		}

		[DoNotSave(0, null)]
		public Upoints MonthlyUnityConsumed => 0.Upoints();

		[DoNotSave(0, null)]
		public Upoints MaxMonthlyUnityConsumed => 0.Upoints();

		public Proto.ID UpointsCategoryId => IdsCore.UpointsCategories.Boost;

		[DoNotSave(0, null)]
		public Option<UnityConsumer> UnityConsumer => m_unityConsumer;
		[DoNotSave(0, null)]
		private UnityConsumer m_unityConsumer;

		[DoNotSave(0, null)]
		public int CurrentInstruction { get; private set; }

		[DoNotSave(0, null)]
		public Electricity PowerRequired { get; private set; } = Electricity.Zero;

		[DoNotSave(0, null)]
		public Option<IElectricityConsumerReadonly> ElectricityConsumer => ((IElectricityConsumerReadonly)m_electricConsumer).SomeOption();
		[DoNotSave(0, null)]
		private IElectricityConsumer m_electricConsumer;

		[DoNotSave(0, null)]
		public Computing ComputingRequired { get; private set; } = Computing.Zero;
		[DoNotSave(0, null)]
		public Option<IComputingConsumerReadonly> ComputingConsumer => ((IComputingConsumerReadonly)m_computingConsumer).SomeOption();

		[DoNotSave(0, null)]
		private IComputingConsumer m_computingConsumer;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationInfoManager;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationErrorManager;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationWarningManager;

		public MaintenanceCosts MaintenanceCosts { get; private set; }

		[DoNotSave(0, null)]
		public IEntityMaintenanceProvider Maintenance => m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private IEntityMaintenanceProvider m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private bool m_reninitNotification;

		private int m_clockSpeed;
		private int m_clock;

		[DoNotSave(0, null)]
		public bool IsIdleForMaintenance => m_maintenanceConsumer.Status.IsBroken;

		[DoNotSave(0, null)]
		public string ErrorMessage { get; private set; }

		[DoNotSave(0, null)]
		public bool IsDebug { get; private set; }

		[DoNotSave(0, null)]
		public bool WaitForUser { get; private set; }

		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT1;
		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT2;
		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT3;

		// TODO handle computation speed quicker than ticks
		public void SimUpdate()
		{
			if (IsNotEnabled && IsNotPaused)
			{
				return;
			}

			if (Modules.Count == 0)
			{
				PowerRequired = Prototype.IddlePower;
				m_electricConsumer.OnPowerRequiredChanged();
				ComputingRequired = Computing.Zero;
				m_computingConsumer.OnComputingRequiredChanged();
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
				return;
			}

			if (IsPaused) {
				return;
			}

			Electricity requiredRunningPower = GetRequiredRunningPower();
			PowerRequired = Prototype.IddlePower + requiredRunningPower;
			m_electricConsumer.OnPowerRequiredChanged();

			Computing requiredComputingPower = GetRequiredComputation();
			ComputingRequired = requiredComputingPower;
			m_computingConsumer.OnComputingRequiredChanged();

			float slotsMaintenance = 0.01f / Prototype.Columns;
			PartialQuantity quantity = new PartialQuantity(
					(Modules.Select(m => m.Layout.GetWidth(m)).Sum() * slotsMaintenance + 0.01f).ToFix32());

			VirtualProductProto lastMaintenanceProduct = Maintenance.Costs.Product;
			VirtualProductProto maintenanceProduct
				= ComputingRequired > 10.TFlops()
				? m_productMaintenanceT3
				: ComputingRequired.IsPositive
				? m_productMaintenanceT2
				: m_productMaintenanceT1;
			var newCosts = new MaintenanceCosts(maintenanceProduct, quantity);
			if (newCosts.MaintenancePerMonth != MaintenanceCosts.MaintenancePerMonth
				|| newCosts.Product != lastMaintenanceProduct)
			{
				MaintenanceCosts = newCosts;
				Maintenance.RefreshMaintenanceCost();
			}

			bool electricityConsumed = m_electricConsumer.TryConsume();

			bool computingConsumed = electricityConsumed && m_computingConsumer.TryConsume();

			if (m_clock >= m_clockSpeed)
			{
				m_clock = 0;
			}
			else
			{
				m_clock++;
				return;
			}

			if (electricityConsumed)
			{
				if (m_maintenanceConsumer.Status.CurrentBreakdownChance < new Random().Next(100).Percent())
				{
					UpdateModules(computingConsumed);
				}
			}
			else
			{
				State = Tr.EntityElectricityConsumptionTooltip__NotEnough;
			}
		}

		private Electricity GetRequiredRunningPower()
		{
			var total = Modules
				.ToArray()
				.Where(m => m.IsNotPaused())
				.Where(m => !(m.Prototype is null))
				.Select(m => m.Prototype.UsedPower.Value)
				.Sum();

			if (DelayBetweenTicks == 0) {
				return total.Kw();
			}

			return (total * 1 / (1 + DelayBetweenTicks)).Max(1).Kw();
		}

		private Computing GetRequiredComputation()
		{
			PartialQuantity sum = PartialQuantity.Zero;
			foreach (Module module in Modules)
			{
				if (module.IsPaused) {
					continue;
				}
				// DynamicComputing wins over the static UsedComputing when set
				// (PLC modules: cost = 1 + 0.05 × parsed-node-count).  Falling
				// back to UsedComputing keeps every other module unchanged.
				sum += module.Prototype.DynamicComputing != null
					? module.Prototype.DynamicComputing(module)
					: module.Prototype.UsedComputing;
			}

			if (sum == PartialQuantity.Zero) {
				return Computing.Zero;
			}
			sum = (sum * 1 / (1 + DelayBetweenTicks)).Max(PartialQuantity.One);
			return Computing.FromQuantity(sum.IntegerPart.Max(Quantity.One));
		}

		private void UpdateModules(bool computingConsumed)
		{
			// This will run the "compiled tree" per tick
			// the tree is recompiled when edited only or when construct
			Dictionary<long, Module> cache = Modules.ToDictionary(m => m.Id);

			// Copy all outputs to inputs
			foreach (Module module in Modules)
			{
				// Clear last-tick values across all effective inputs (statics + active
				// extensions) so unconnected pins evaluate as zero on the next read.
				foreach (var input in module.EffectiveInputs)
				{
					module.InputNumberData.TryRemove(input.Id, out _);
					// module.StringData.TryRemove("in__" + input.Id, out _);
				}

				foreach (KeyValuePair<string, ModuleConnector> item in module.InputModules.ToArray())
				{
					if (cache.TryGetValue(item.Value.ModuleId, out Module connected))
					{
						module.Input[item.Key] = connected.Output[item.Value.OutputId, 0];
					}
					else
					{
						// Remove disconnected module
						module.InputModules.Remove(item.Key);
						module.Input[item.Key] = Fix32.Zero;
					}
				}
			}

			// Execute all modules
			bool anyError = false;
			bool anyWarning = false;
			bool anyInfo = false;
			bool missingComputation = false;
			foreach (Module module in Modules)
			{
				try
				{
					if (module.Unlocked == false) {
						// Only skip, UI will show automatically
						module.SetStatus(ModuleStatus.Skipped);
						continue;
					}
					// Match the GetRequiredComputation path: a module needs computing
					// if its dynamic-cost callback returns >0 OR (when no callback)
					// its static UsedComputing is >0.  Without this, PLC modules
					// (whose cost is always dynamic) would never skip on missing
					// computing because their UsedComputing stays at zero.
					PartialQuantity moduleCost = module.Prototype.DynamicComputing != null
						? module.Prototype.DynamicComputing(module)
						: module.Prototype.UsedComputing;
					if (moduleCost > PartialQuantity.Zero && !computingConsumed)
					{
						missingComputation = missingComputation || true;
						module.SetStatus(ModuleStatus.Skipped);
						continue;
					}

					if (module.Status == ModuleStatus.Skipped) {
						module.SetStatus(ModuleStatus.Init);
					}
					module.Execute();
				}
				catch (Exception e)
				{
					anyError = true;
					if (module.IsDebugging) {
						Log.Exception(e);
					}
					// ignore exception
				}
				anyInfo = anyInfo || module.Info;
				anyWarning = anyWarning || module.Warning;
				anyError = anyError || module.Status == ModuleStatus.Error;
			}
			if (anyError)
			{
				m_notificationErrorManager.Activate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
			}
			else if (anyWarning)
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Activate(this);
				m_notificationInfoManager.Deactivate(this);
			}
			else if (anyInfo)
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Activate(this);
			}
			else
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
			}

			State = Tr.EntityStatus__Working;
		}

		public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
		{
			return Quantity.Zero; // TODO keep displayed content
		}

		[DoNotSave()]
		public Lyst<Module> Modules { get; private set; }

		// Holds the layout table read from a pre-MODULE_LAYOUT_INFO save until
		// initContexts can back-fill module positions; cleared right after.
		[DoNotSave(0, null)]
		private Lyst<Lyst<ModulePlacement>> m_legacyRows;

		// Computed grid view over Modules' (Row, Column). Recomputed on every read; do not
		// mutate the returned Lyst — it is a snapshot. Kept for line-painting / read-only consumers.
		[DoNotSave()]
		public Lyst<Lyst<ModulePlacement>> Rows
		{
			get
			{
				int rows = Prototype != null ? Prototype.Rows : 0;
				int cols = Prototype != null ? Prototype.Columns : 0;
				var grid = new Lyst<Lyst<ModulePlacement>>();
				for (int i = 0; i < rows; i++)
				{
					var row = new Lyst<ModulePlacement>();
					for (int j = 0; j < cols; j++)
					{
						row.Add(ModulePlacement.Empty);
					}
					grid.Add(row);
				}
				if (Modules == null) {
					return grid;
				}
				foreach (var m in Modules)
				{
					if (m == null || m.Prototype == null) {
						continue;
					}
					int width = m.Layout.GetWidth(m);
					if (m.Row < 0 || m.Row >= grid.Count) {
						continue;
					}
					var row = grid[m.Row];
					for (int x = 0; x < width; x++)
					{
						int c = m.Column + x;
						if (c < 0 || c >= row.Count) {
							continue;
						}
						row[c] = x == 0 ? ModulePlacement.Origin(m.Id) : ModulePlacement.Rest(m.Id);
					}
				}
				return grid;
			}
		}

		[DoNotSave()]
		public int GeneralPriority { get; set; }

		[DoNotSave()]
		public bool IsGeneralPriorityVisible => true;

		[DoNotSave()]
		public bool IsCargoAffectedByGeneralPriority => false;

		[DoNotSave()]
		public int DelayBetweenTicks { get => m_clockSpeed; set => m_clockSpeed = value; }

		[DoNotSave()]
		public int Clock { get => m_clock; set => m_clock = value; }
		public LocStrFormatted State { get; private set; }
		public ColorRgba Color { get; private set; }

		[DoNotSave(resolveAfterLoad: typeof(DependencyResolver))]
		public DependencyResolver Resolver { get; set; }
		public void SetColor(ColorRgba color)
		{
			Color = color;
		}
	}
}
