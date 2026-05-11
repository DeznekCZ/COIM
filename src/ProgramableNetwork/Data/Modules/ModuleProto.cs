using Mafi.Serialization;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Base;
using Mafi.Core.Entities.Static;
using System.Linq;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Unity.UiToolkit.Component;
using ProgramableNetwork.ModuleParser.Registrator.Definitions;
using ProgramableNetwork.Ui;

namespace ProgramableNetwork
{
    public class ModuleProto : EntityProto, IProtoWithIcon
    {

        [DebuggerStepThrough]
        [DebuggerDisplay("{Value,nq}")]
        [ManuallyWrittenSerialization]
        public new readonly struct ID : IEquatable<ID>, IComparable<ID>
        {
            //
            // Souhrn:
            //     Underlying string value of this Id.
            public readonly string Value;

            public ID(string value)
            {
                Value = value;
            }

            public static bool operator ==(ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public override bool Equals(object other)
            {
                if (other is ID)
                {
                    ID other2 = (ID)other;
                    return Equals(other2);
                }

                return false;
            }

            public bool Equals(ID other)
            {
                return string.Equals(Value, other.Value, StringComparison.Ordinal);
            }

            public int CompareTo(ID other)
            {
                return string.CompareOrdinal(Value, other.Value);
            }

            public override string ToString()
            {
                return Value ?? string.Empty;
            }

            public override int GetHashCode()
            {
                return Value?.GetHashCode() ?? 0;
            }

            public static void Serialize(ID value, BlobWriter writer)
            {
                writer.WriteString(value.Value);
            }

            public static ID Deserialize(BlobReader reader)
            {
                return new ID(reader.ReadString());
            }

            public static implicit operator Proto.ID(ID id)
            {
                return new Proto.ID(id.Value);
            }

            public static implicit operator EntityProto.ID(ID id)
            {
                return new EntityProto.ID(id.Value);
            }
        }


        public new class Gfx : EntityProto.Gfx
        {
            public static new readonly Gfx Empty;

            public string IconPath { get; }

            public Gfx(string iconPath, ColorRgba? color = null)
                : base(color ?? ColorRgba.White)
            {
                this.IconPath = iconPath;
            }

            static Gfx()
            {
                Empty = new Gfx(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png, ColorRgba.Empty);
            }
        }

        public new ID Id { get; }
        public Func<Module, ModuleStatus> Action { get; }
        public Func<Module, ModuleStatus> Init { get; }
        public bool IsInputModule { get; }
        public bool IsOutputModule { get; }
        public List<ModuleConnectorProto> Inputs { get; }
        public List<ModuleConnectorProto> Outputs { get; }
        // Optional pin extensions appended to the right side of the module when the
        // player adds them through the inspector.  Pre-materialised at proto
        // registration so the LocStr table contains every possible extension's
        // strings up front (Strs cannot be added after registration).  A module
        // instance carries an InputExtensionCount / OutputExtensionCount that
        // selects the first N entries of these lists at runtime.
        public List<ModuleConnectorProto> InputExtensions { get; }
        public List<ModuleConnectorProto> OutputExtensions { get; }
        public int MaxInputExtensions => InputExtensions?.Count ?? 0;
        public int MaxOutputExtensions => OutputExtensions?.Count ?? 0;
        // Trailing input pins are rendered at the RIGHT edge after any active
        // extensions — used for fallback/else-style pins that should stay
        // visually anchored to the far end regardless of how many extensions
        // the player has added.  Layout: [statics] [extensions] [trailings].
        // Trailings are always present (not gated by an extension counter).
        public List<ModuleConnectorProto> InputsTrailing { get; }
        // Maximum number of cells the LAST display in <see cref="Displays"/> can grow by
        // when the player adds display extensions through the inspector.  Unlike pin
        // extensions, display extensions don't add new ModuleConnectorProto entries —
        // they just grow the rightmost display's rendered width by N cells.  Modules
        // that opt into this typically have a single value-display (e.g. Display_Int).
        public int MaxDisplayExtensions { get; }
        public List<ModuleConnectorProto> Displays { get; }
        // Optional per-output (or per-input) display widgets that materialise
        // alongside their matching pin extension.  Used by paired pin-and-LED
        // modules like the flip-flop: declaring `extension_displays` linked to
        // "output" causes one extra display to be rendered whenever the player
        // adds an output extension on the right edge.  Indexed in lock-step
        // with the active linked-side extension count.
        public List<ModuleConnectorProto> ExtensionDisplays { get; }
        public ExtensionSide ExtensionDisplaysLinkedSide { get; }

        /// <summary>
        /// When true, growing/shrinking the input pin extensions also moves the
        /// output extensions in lock-step (and vice versa) — used by paired-channel
        /// modules like flip-flop where every <c>in_N</c> must always have a
        /// matching <c>out_N</c>.  Enforced in the command executor: a single
        /// <c>SetExtensionCount</c> call mirrors the count to the linked side.
        /// </summary>
        public bool LinkInputOutputExtensions { get; }
        public Action<Module> DisplayUpdate { get; }
        public ImmutableArray<Category> Categories { get; }
        public List<IField> Fields { get; }
        public Electricity UsedPower { get; }
        public int BaseWidth { get; }
        public PartialQuantity UsedComputing { get; }
        // Optional per-instance computing override.  When set, Controller's
        // GetRequiredComputation calls this instead of using the static
        // UsedComputing — used by the PLC module so each instance's cost
        // scales with its parsed lexer-node count.  Leaving it null keeps
        // the existing single-value-per-prototype behavior for every other
        // module.
        public Func<Module, PartialQuantity> DynamicComputing { get; }
        public Action<Module, UiComponent> DisplayFunction { get; }
        public Func<Module, int> WidthFunction { get; }

        public override Type EntityType => typeof(Module);

        public new Gfx Graphics { get; }
        public string IconPath => Graphics.IconPath;

        public string Symbol { get; }
        public List<StaticEntityProto.ID> AllowedDevices { get; }
		public ImmutableArray<ResearchNodeProto> ResearchDependency { get; set; }

        public static readonly ModuleProto Phantom;
        protected static readonly ID PHANTOM_PRODUCT_ID;

        static ModuleProto() {
            try
            {
                PHANTOM_PRODUCT_ID = new ID("__PHANTOM__MODULE__");
                Phantom = new Builder(null, PHANTOM_PRODUCT_ID)
                    .SetDescription("Module replacement for already nonexsiting module")
                    .SetGfx(Assets.Base.Products.Icons.Vegetables_svg)
                    .SetSymbol("!!")
                    .SetName("[Removed module]")
                    .Action(m => ModuleStatus.Error)
                    //.AddDisplay("__display__", "Index", 1)
                    //.AddDisplay("__display__", "Display of keys", 5)
                    //.Action((module) =>
                    //{
                    //    if (module.IsDebugging)
                    //    {
                    //        var stringKeys = module.StringData
                    //            .Where(k => k != "display____display__")
                    //            .ToArray();
                    //
                    //        var intKeys = module.NumberData
                    //            .Where(k => k != "display____display__")
                    //            .ToArray();
                    //    }
                    //    return ModuleStatus.Error;
                    //})
                    .Build();
                typeof(ModuleProto)
                    .GetField("<WidthFunction>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(Phantom, (Func<Module, int>)((m) => 1));

            }
            catch (Exception e)
            {
                Log.Exception(e);
                throw;
            }
        }

        public ModuleProto(ID id, Str strings, EntityCosts costs, Gfx gfx,
			IEnumerable<Tag> tags, Func<Module, ModuleStatus> action, Func<Module, ModuleStatus> m_init,
			Action<Module> m_display, bool isInputModule, bool isOutputModule, Electricity usedPower,
			PartialQuantity usedComputing, List<ModuleConnectorProto> m_inputs, List<ModuleConnectorProto> m_outputs,
			List<ModuleConnectorProto> m_displays, List<IField> m_fields,
			Action<Module, UiComponent> m_displayFunction, int baseWidth, Func<Module, int> m_widthFunction,
			string m_symbol, List<StaticEntityProto.ID> m_allowedDevices, List<Category> m_categories,
            ImmutableArray<ResearchNodeProto> m_research,
            List<ModuleConnectorProto> m_inputExtensions = null,
            List<ModuleConnectorProto> m_outputExtensions = null,
            int m_maxDisplayExtensions = 0,
            List<ModuleConnectorProto> m_extensionDisplays = null,
            ExtensionSide m_extensionDisplaysLinkedSide = ExtensionSide.Output,
            Func<Module, PartialQuantity> m_dynamicComputing = null,
            bool m_linkInputOutputExtensions = false,
            List<ModuleConnectorProto> m_inputsTrailing = null
		) : base(id, strings, costs, gfx, tags)
        {
            Id = id;
            Symbol = m_symbol;
            Action = action;
            Init = m_init;
            IsInputModule = isInputModule;
            IsOutputModule = isOutputModule;
            Inputs = m_inputs;
            Outputs = m_outputs;
            InputExtensions = m_inputExtensions ?? new List<ModuleConnectorProto>();
            OutputExtensions = m_outputExtensions ?? new List<ModuleConnectorProto>();
            InputsTrailing = m_inputsTrailing ?? new List<ModuleConnectorProto>();
            MaxDisplayExtensions = System.Math.Max(0, m_maxDisplayExtensions);
            ExtensionDisplays = m_extensionDisplays ?? new List<ModuleConnectorProto>();
            ExtensionDisplaysLinkedSide = m_extensionDisplaysLinkedSide;
            LinkInputOutputExtensions = m_linkInputOutputExtensions;
            Displays = m_displays;
            DisplayUpdate = m_display;
            Fields = m_fields;
            UsedPower = usedPower;
            UsedComputing = usedComputing;
            DynamicComputing = m_dynamicComputing;
            Graphics = gfx;
            DisplayFunction = m_displayFunction;
            WidthFunction = m_widthFunction;
            // Auto-width includes trailing inputs since they reserve a cell each
            // even without extensions (they always render at the right edge).
            BaseWidth = baseWidth > 0 ? baseWidth
                :    (Inputs.Count + InputsTrailing.Count)
                .Max(Outputs.Count)
                .Max(Fields.Count)
                .Max(Displays.Select(d => d.Width).Sum(d => d.ToFloat()).RoundToInt())
                ;
            AllowedDevices = m_allowedDevices;
            Categories = m_categories.ToImmutableArray();
			ResearchDependency = m_research;
            SetAvailability(false);
        }

		public class Builder
        {
            private readonly List<Tag> m_tags = new List<Tag>();
            private ProtoRegistrator m_registrator;
            private readonly ID m_id;
            private string m_name;
            private string m_description;
            private string m_hint = "";
            private Func<Module, ModuleStatus> m_action;
            private Func<Module, ModuleStatus> m_init;
            private Action<Module> m_display;
            private bool m_isOutputModule = false;
            private bool m_isInputModule = false;
            private readonly List<ModuleConnectorProto> m_inputs = new List<ModuleConnectorProto>();
            private readonly List<ModuleConnectorProto> m_outputs = new List<ModuleConnectorProto>();
            private readonly List<ModuleConnectorProto> m_inputExtensions = new List<ModuleConnectorProto>();
            private readonly List<ModuleConnectorProto> m_outputExtensions = new List<ModuleConnectorProto>();
            // Trailing inputs rendered at the right edge after any extensions —
            // intended for fallback/else-style pins. See ModuleProto.InputsTrailing.
            private readonly List<ModuleConnectorProto> m_inputsTrailing = new List<ModuleConnectorProto>();
            private int m_maxDisplayExtensions;
            // Lock-step display widgets — one per active linked-side extension.
            // Currently only Output linkage is wired; Input linkage falls through
            // identically but isn't exercised by any module yet.
            private readonly List<ModuleConnectorProto> m_extensionDisplays = new List<ModuleConnectorProto>();
            private ExtensionSide m_extensionDisplaysLinkedSide = ExtensionSide.Output;
            private bool m_linkInputOutputExtensions;
            // When non-null, AddDisplay/AddDisplayFiller/AddDisplaySlider route their
            // entries here instead of the main Displays list.  Used by
            // AddExtensionDisplays to capture per-extension display widgets without
            // duplicating each AddXxx overload.
            private List<ModuleConnectorProto> m_displaysTargetOverride;
            private readonly List<ModuleConnectorProto> m_displays = new List<ModuleConnectorProto>();
            private Electricity m_usedPower;
            private PartialQuantity m_usedComputing;
            private Func<Module, PartialQuantity> m_dynamicComputing;
            private EntityCostsTpl.Builder m_costs;
            private Gfx m_gfx;
            private string m_symbol;
            private readonly List<IField> m_fields = new List<IField>();
            private readonly List<StaticEntityProto.ID> m_allowedDevices;
            private bool m_customBuild;
            private bool m_customMaintenance;
            private List<Category> m_categories = new List<Category>();
            private int m_baseWidth;
			private Lyst<ResearchNodeProto.ID> m_researchIds = [];

			public Action<Module, UiComponent> m_displayFunction { get; }
            public Func<Module, int> m_widthFunction { get; private set; }

            public Builder(ProtoRegistrator registrator, string id, string name, string description, string symbol, Gfx gfx)
            {
                m_registrator = registrator;
                m_id = new ID(id.ModuleId());
                m_name = name;
                m_description = description;
                m_tags = new List<Tag>();
                m_usedPower = 0.Kw();
                m_costs = new EntityCostsTpl.Builder();
                m_gfx = gfx;
                m_symbol = symbol;
                m_allowedDevices = new List<StaticEntityProto.ID>();
            }

            public Builder(ProtoRegistrator registrator, ID id)
            {
                m_registrator = registrator;
                m_id = id;
                m_tags = new List<Tag>();
                m_usedPower = 0.Kw();
                m_costs = new EntityCostsTpl.Builder();
                m_allowedDevices = new List<StaticEntityProto.ID>();
            }

            public Builder(ProtoRegistrator registrator, string id)
            {
                m_registrator = registrator;
                m_id = new ID(id.ModuleId());
                m_tags = new List<Tag>();
                m_usedPower = 0.Kw();
                m_costs = new EntityCostsTpl.Builder();
                m_allowedDevices = new List<StaticEntityProto.ID>();
            }

            public ModuleProto Build()
            {
                if (!m_customMaintenance) {
					UseDefaultMaintenance();
				}
				if (!m_customBuild) {
					BuildDefault();
				}

				return new ModuleProto(
                    m_id,
                    CreateStr(m_id, m_name, m_description, m_hint),
                    m_registrator == null ? new EntityCosts() : ((EntityCostsTpl)m_costs).MapToEntityCosts(m_registrator),
                    m_gfx,
                    m_tags,
                    m_action ?? (m => ModuleStatus.Running),
                    m_init ?? (m => ModuleStatus.Running),
                    m_display ?? (m => { }),
                    m_isInputModule,
                    m_isOutputModule,
                    m_usedPower,
                    m_usedComputing,
                    m_inputs,
                    m_outputs,
                    m_displays,
                    m_fields,
                    m_displayFunction,
                    m_baseWidth,
                    m_widthFunction,
                    m_symbol,
                    m_allowedDevices,
                    m_categories,
                    m_researchIds
						.Select(id => m_registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
						.ToImmutableArray(),
                    m_inputExtensions,
                    m_outputExtensions,
                    m_maxDisplayExtensions,
                    m_extensionDisplays,
                    m_extensionDisplaysLinkedSide,
                    m_dynamicComputing,
                    m_linkInputOutputExtensions,
                    m_inputsTrailing
                );
            }

            public ModuleProto BuildAndAdd()
            {
                return BuildAndAdd(NewIds.Research.ProgramableNetwork_Stage1);
            }

            public ModuleProto BuildAndAdd(ResearchNodeProto.ID researchStage)
            {
                Research.AddModule(m_id, researchStage);
                return m_registrator.PrototypesDb.Add(Build());
            }

            public Builder SetName(string name)
            {
                this.m_name = name;
                return this;
            }

            public Builder SetDescription(string description)
            {
                this.m_description = description;
                return this;
            }

            /// <summary>
            /// Translator-context comment attached to the module's name and description.
            /// Not shown to the player; surfaced to translators so they understand what the module does.
            /// </summary>
            public Builder SetHint(string hint)
            {
                this.m_hint = hint ?? "";
                return this;
            }

            public Builder SetSymbol(string symbol)
            {
                this.m_symbol = symbol;
                return this;
            }

            public Builder SetGfx(string gfx)
            {
                this.m_gfx = new Gfx(gfx);
                return this;
            }

            public Builder AddTag(Tag tag)
            {
                m_tags.Add(tag);
                return this;
            }

            public Builder AddInput(string id, string name)
            {
                m_inputs.Add(new ModuleConnectorProto(id, m_id.Input(id, name), 1));
                return this;
            }

            /// <summary>
            /// Shared variant — the label is registered ONCE under
            /// <c>ProgramableNetwork_PinOrField_&lt;name&gt;</c> and reused across every
            /// module that calls <c>.Shared()</c> with the same string. Mints no
            /// per-module key, so translation files don't accumulate one entry per
            /// module per duplicated label.
            /// </summary>
            public Builder AddInput(string id, SharedLabel shared)
            {
                m_inputs.Add(new ModuleConnectorProto(id, shared.Resolve(), 1));
                return this;
            }

            /// <summary>
            /// Adds an input pin that renders at the right edge of the module
            /// AFTER any active extensions — intended for fallback/else-style
            /// pins that should stay visually anchored at the far end.
            /// Trailing pins are always present (no extension counter) but their
            /// column shifts right as the player adds extensions, so a wire to
            /// the trailing pin keeps landing on the same logical role no matter
            /// how the module grows.
            /// </summary>
            public Builder AddInputTrailing(string id, string name)
            {
                m_inputsTrailing.Add(new ModuleConnectorProto(id, m_id.Input(id, name), 1));
                return this;
            }

            /// <summary>Shared-label variant of <see cref="AddInputTrailing(string, string)"/>.</summary>
            public Builder AddInputTrailing(string id, SharedLabel shared)
            {
                m_inputsTrailing.Add(new ModuleConnectorProto(id, shared.Resolve(), 1));
                return this;
            }

            public Builder AddOutput(string id, string name)
            {
                m_outputs.Add(new ModuleConnectorProto(id, m_id.Output(id, name), 1));
                return this;
            }

            /// <summary>
            /// Shared variant — see <see cref="AddInput(string, SharedLabel)"/>.
            /// </summary>
            public Builder AddOutput(string id, SharedLabel shared)
            {
                m_outputs.Add(new ModuleConnectorProto(id, shared.Resolve(), 1));
                return this;
            }

            /// <summary>
            /// Declares that this module supports up to <paramref name="max"/> extra input pins
            /// added by the player from the inspector.  All <paramref name="max"/> Strs are
            /// pre-registered up front (Strs cannot be added after proto registration); the
            /// active count per instance is held on the Module.  Default <paramref name="namer"/>
            /// continues the alphabet sequence of the last static input id (e.g. existing 'a','b'
            /// → extensions 'c','d','...'); pass a custom namer for ids that don't fit a
            /// single-letter pattern.
            /// </summary>
            public Builder AllowInputExtensions(int max, Func<int, (string id, string name)> namer = null)
            {
                if (max <= 0) {
                    return this;
                }
                namer ??= defaultExtensionNamer(m_inputs);
                for (int i = 0; i < max; i++)
                {
                    var (id, name) = namer(i);
                    m_inputExtensions.Add(new ModuleConnectorProto(id, m_id.Input(id, name), 1));
                }
                return this;
            }

            /// <summary>
            /// Shared variant of <see cref="AllowInputExtensions"/> — every extension
            /// pin's display label resolves through the shared registry rather than
            /// being minted per-module. Use when extensions reuse common labels
            /// like single letters that other modules also expose.
            /// </summary>
            public Builder AllowInputExtensionsShared(int max, Func<int, (string id, SharedLabel shared)> namer)
            {
                if (max <= 0 || namer == null) {
                    return this;
                }
                for (int i = 0; i < max; i++)
                {
                    var (id, shared) = namer(i);
                    m_inputExtensions.Add(new ModuleConnectorProto(id, shared.Resolve(), 1));
                }
                return this;
            }

            public Builder AllowOutputExtensions(int max, Func<int, (string id, string name)> namer = null)
            {
                if (max <= 0) {
                    return this;
                }
                namer ??= defaultExtensionNamer(m_outputs);
                for (int i = 0; i < max; i++)
                {
                    var (id, name) = namer(i);
                    m_outputExtensions.Add(new ModuleConnectorProto(id, m_id.Output(id, name), 1));
                }
                return this;
            }

            /// <summary>
            /// Shared variant of <see cref="AllowOutputExtensions"/> — see
            /// <see cref="AllowInputExtensionsShared"/>.
            /// </summary>
            public Builder AllowOutputExtensionsShared(int max, Func<int, (string id, SharedLabel shared)> namer)
            {
                if (max <= 0 || namer == null) {
                    return this;
                }
                for (int i = 0; i < max; i++)
                {
                    var (id, shared) = namer(i);
                    m_outputExtensions.Add(new ModuleConnectorProto(id, shared.Resolve(), 1));
                }
                return this;
            }

            /// <summary>
            /// Lock-step input and output pin extensions: a single
            /// <see cref="Module.SetExtensionCountLinked"/> call mirrors the new count
            /// to whichever side wasn't asked, so a player adding an input pin to a
            /// flip-flop also gets the matching output pin (and vice versa).
            /// Caller is expected to register the same <c>AllowInputExtensions</c> and
            /// <c>AllowOutputExtensions</c> max so the linked sides can keep up.
            /// </summary>
            public Builder LinkInputOutputExtensions()
            {
                m_linkInputOutputExtensions = true;
                return this;
            }

            /// <summary>
            /// Declares that the LAST display in <see cref="AddDisplay"/> can grow by up to
            /// <paramref name="max"/> additional cells when the player adds display
            /// extensions through the inspector.  Unlike pin extensions, no new
            /// ModuleConnectorProto entries are registered — the existing display widget
            /// just stretches.  Use this on display-dominant modules where the player
            /// chooses precision (e.g. number displays growing from 4 to 16 digits).
            /// </summary>
            public Builder AllowDisplayExtensions(int max)
            {
                m_maxDisplayExtensions = System.Math.Max(0, max);
                return this;
            }

            /// <summary>
            /// Registers per-extension display widgets that follow a pin side.  Each entry
            /// in <paramref name="extensions"/> becomes a display added to the row at the
            /// same active count as the linked side's pin extensions — e.g. a flip-flop
            /// linked to Output, with one LED per channel.  Calls inside the lambdas
            /// (AddDisplay / Display.LED / etc.) are captured into the extension list
            /// instead of the main displays list via a thread-unsafe target swap, so
            /// don't interleave with other Add* calls.
            /// </summary>
            public Builder AllowExtensionDisplays(ExtensionSide linkedSide, IEnumerable<DisplayConstructorAction> extensions)
            {
                m_extensionDisplaysLinkedSide = linkedSide;
                m_displaysTargetOverride = m_extensionDisplays;
                try
                {
                    foreach (DisplayConstructorAction ext in extensions)
                    {
                        ext(this);
                    }
                }
                finally
                {
                    m_displaysTargetOverride = null;
                }
                return this;
            }

            // Default extension namer: continues from the next ASCII character after the
            // last static pin's id when that id is exactly one alphanumeric character.
            // So "B" → "C", "D"...; "1" → "2", "3"...; "D" → "E", "F".  Falls back to
            // 'A'+i if there are no statics or the last id isn't a single alnum char —
            // modules with multi-char pin ids must pass an explicit namer.
            private static Func<int, (string id, string name)> defaultExtensionNamer(List<ModuleConnectorProto> existing)
            {
                int startCode = 'A';
                if (existing.Count > 0)
                {
                    string lastId = existing[existing.Count - 1].Id;
                    if (lastId != null && lastId.Length == 1 && (
                        (lastId[0] >= 'a' && lastId[0] <= 'z') ||
                        (lastId[0] >= 'A' && lastId[0] <= 'Z') ||
                        (lastId[0] >= '0' && lastId[0] <= '9')))
                    {
                        startCode = lastId[0] + 1;
                    }
                }
                return idx =>
                {
                    string id = ((char)(startCode + idx)).ToString();
                    // Display label uppercases the id so an extension following
                    // lowercase static pins ("a","b") still shows as "C","D" in
                    // the inspector. Translation files (legacy Boolean_And_4 etc.)
                    // already use uppercase labels — this keeps the auto-namer
                    // consistent with that intent.
                    string name = id.ToUpperInvariant();
                    return (id, name);
                };
            }

            public Builder AddDevice(StaticEntityProto.ID device)
            {
                m_allowedDevices.Add(device);
                return this;
            }

            public Builder AddControllerDevice()
            {
                return AddDevice(NewIds.Controllers.Controller);
            }

            public Builder UsePower(Electricity usedPower)
            {
                m_usedPower = usedPower;
                return this;
            }

            public Builder UseMaintenance(VirtualProductProto.ID maintenance, int count)
            {
                m_customMaintenance = true;
                m_costs.Maintenance(count, maintenance);
                return this;
            }

            public Builder UseDefaultMaintenance()
            {
                return UseMaintenance(Ids.Products.MaintenanceT1, 1);
            }

            public Builder BuildProduct(ProductProto.ID product, int count)
            {
                m_customBuild = true;
                m_costs.Product(count, product);
                return this;
            }

            public Builder BuildElectronicsT1(int count = 1)
            {
                return BuildProduct(Ids.Products.Electronics, count);
            }

            public Builder BuildElectronicsT2(int count = 1)
            {
                return BuildProduct(Ids.Products.Electronics2, count);
            }

            public Builder BuildElectronicsT3(int count = 1)
            {
                return BuildProduct(Ids.Products.Electronics3, count);
            }

            public Builder BuildDefault()
            {
                return BuildElectronicsT1(1);
            }

            /// <summary>
            /// Displays will be shown on the name, the name will be overriden
            /// </summary>
            /// <param name="id">indexing name in Module.Field[id]</param>
            /// <param name="name">Displayerd tooltip value</param>
            /// <param name="width">taken module width</param>
            /// <returns></returns>
            public Builder AddDisplay(string id, string name, Fix32 width, string defaultText = null, bool image = false, string[] toggle = null, bool entity = false, bool led = false)
            {
                (m_displaysTargetOverride ?? m_displays).Add(new ModuleConnectorProto(id, m_id.Display(id, name), width,
                    defaultText ?? (
                    image ? "[image]" :
                    led ? "[led]":
                    toggle != null ? "[toggle]" + WriteToggleArray(toggle) :
                    (new string('0', width.IntegerPart * 2) + "|")
                    )));
                return this;
            }
            public Builder AddDisplayFiller(Fix32 width)
            {
                (m_displaysTargetOverride ?? m_displays).Add(new ModuleConnectorProto("_", Str.Empty, width, "[fill]"));
                return this;
            }

            /// <summary>
            /// Slider display — reads its current value from <c>module.Display[id]</c> and
            /// the slider bounds from <c>module.Display[id + "_min"]</c> /
            /// <c>module.Display[id + "_max"]</c> at runtime, falling back to the defaults
            /// passed here.  Width is in module cells (1–4).  Render is non-interactive
            /// (it's a display, not an input).
            /// TODO(displays): the whole display API needs richer fields (cable-style
            /// metadata for binding, not stringly-typed DefaultText parsing).  This entry
            /// is a placeholder — deferred for later.
            /// </summary>
            public Builder AddDisplaySlider(string id, string name, Fix32 width, float min = 0f, float max = 1f)
            {
                string defaultText = "[slider]:"
                    + min.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + ":"
                    + max.ToString(System.Globalization.CultureInfo.InvariantCulture);
                (m_displaysTargetOverride ?? m_displays).Add(new ModuleConnectorProto(id, m_id.Display(id, name), width, defaultText));
                return this;
            }

            public Builder AddDisplayFromPython(DisplayConstructorAction proto)
            {
                proto(this);
                return this;
            }

            private string WriteToggleArray(string[] toggle)
            {
                char separator = "|`#,".AsEnumerable().Where(c => !toggle.SelectMany(t => t).Any(t => t == c)).First();
                return separator + string.Join("" + separator, toggle);
            }

            public Builder Action(Action<Module> action)
            {
                m_action = (m) =>
                {
                    action(m);
                    return ModuleStatus.Running;
                };
                return this;
            }

            public Builder Action(Func<Module, ModuleStatus> action)
            {
                m_action = action;
                return this;
            }

            public Builder Display(Action<Module> action)
            {
                m_display = action;
                return this;
            }

            public Builder AddCategory(Category category)
            {
                m_categories.Add(category);
                return this;
            }

			public Builder UnlockedBy(ResearchNodeProto.ID researchId) {
                m_researchIds.Add(researchId);
                return this;
			}

            /// <summary>
            /// Overrides default width calculation
            /// </summary>
            /// <param name="slots"></param>
            /// <returns></returns>
            public Builder Width(int slots)
            {
                m_baseWidth = slots;
                return this;
            }

            /// <summary>
            /// Dynamic width definition
            /// TODO make an reaction for extension
            /// TODO make replacer for modules, that are converted to dynamic width
            /// </summary>
            /// <param name="slots"></param>
            /// <returns></returns>
            public Builder Width(Func<Module, int> slots)
            {
                m_widthFunction = slots;
                return this;
            }

            public Builder Init(Action<Module> init)
            {
                m_init = (m) => { init(m); return ModuleStatus.Running; };
                return this;
            }

            public Builder Init(Func<Module, ModuleStatus> init)
            {
                m_init = init;
                return this;
            }

            public Builder AddBooleanField(string id, string name, string shortDesc = "", bool defaultValue = false, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new BooleanField(id, m_id.Field(id, name, shortDesc), defaultValue, showInTooltip));
                return this;
            }

            public Builder AddInt32Field(string id, string name, string shortDesc = "", int defaultValue = 0, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new NumberField<int>(id, m_id.Field(id, name, shortDesc), defaultValue, showInTooltip));
                return this;
            }

            /// <summary>
            /// Shared-label variant: field name comes from the shared registry
            /// (<see cref="SharedFieldLabels.Shared(string)"/>) so its translation
            /// key is reused across every module that calls <c>.Shared()</c> with
            /// the same string. ShortDesc is intentionally empty — pair with an
            /// <see cref="AddInfoField(string, string)"/> entry above the field
            /// group when the semantics need a one-line explanation.
            /// <paramref name="linkedToInput"/> ties the row's visibility to an
            /// input pin id — the row hides when the matching extension pin is
            /// not currently active. Useful for threshold/companion fields that
            /// should track their paired extension pin.
            /// </summary>
            public Builder AddInt32Field(string id, SharedLabel shared, int defaultValue = 0, bool overrideInput = false, bool showInTooltip = false, string linkedToInput = null)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new NumberField<int>(id, shared.Resolve(), defaultValue, showInTooltip, linkedToInput));
                return this;
            }

            /// <summary>
            /// Read-only paragraph rendered inline with the rest of the module's
            /// fields. Used to introduce a block of related fields ("the values
            /// below are thresholds, …") so the per-field labels can stay short
            /// (typically just a shared single letter). Holds no data and is
            /// skipped by Validate / InitData.
            /// </summary>
            public Builder AddInfoField(string id, string text)
            {
                m_fields.Add(new InfoField(id, m_id.Field(id, text, "")));
                return this;
            }

            public Builder AddHexInt32Field(string id, string name, string shortDesc = "", uint defaultValue = 0, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new NumberField<HexInt32>(id, m_id.Field(id, name, shortDesc), new HexInt32() { Value = (int)defaultValue }, showInTooltip));
                return this;
            }

            public Builder AddColorField(string id, string name, string shortDesc = "", int defaultValue = 0, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new ColorField(id, m_id.Field(id, name, shortDesc), defaultValue, showInTooltip));
                return this;
            }

            public Builder AddColorField(string id, string name, string shortDesc = "", ColorRgba? defaultValue = null, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new ColorField(id, m_id.Field(id, name, shortDesc), defaultValue ?? new ColorRgba(), showInTooltip));
                return this;
            }

            public Builder AddInt64Field(string id, string name, string shortDesc = "", long defaultValue = 0, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new NumberField<long>(id, m_id.Field(id, name, shortDesc), defaultValue, showInTooltip));
                return this;
            }

            public Builder AddFix32Field(string id, string name, string shortDesc = "", Fix32? defaultValue = null, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new NumberField<Fix32>(id, m_id.Field(id, name, shortDesc), defaultValue ?? Fix32.Zero, showInTooltip));
                return this;
            }

            public Builder AddStringField(string id, string name, string shortDesc = "", string defaultValue = "", bool overrideInput = false, bool multilined = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new StringField(id, m_id.Field(id, name, shortDesc), defaultValue, multilined, showInTooltip));
                return this;
            }

            /// <summary>
            /// Sugar for the "input pin + value field with same id" pattern: registers a companion
            /// BooleanField with id "field_&lt;id&gt;" which DefaultField uses to drive the override
            /// toggle row. Only the same-id case is supported — the runtime <c>FieldOrInput[id]</c>
            /// dispatch reads the boolean stored at "field_&lt;id&gt;" to decide between input and field.
            /// </summary>
            private void addOverrideToggle(string valueFieldId)
            {
                string toggleId = $"field_{valueFieldId}";
                m_fields.Add(new BooleanField(toggleId, m_id.Field(toggleId, $"Override input '{valueFieldId}'", ""), false));
            }

            public Builder AddEntityField(string id, string name, Func<Module, IEntity, bool> entitySelector = null)
            {
                m_fields.Add(new EntityField(id, m_id.Field(id, name), entitySelector, 20.ToFix32()));
                return this;
            }

            public Builder AddEntityField<T>(string id, string name)
                where T : IEntity
            {
                m_fields.Add(new EntityField(id, m_id.Field(id, name), (module, entity) => entity is T, 20.ToFix32()));
                return this;
            }

            public Builder AddEntityField<T>(string id, string name, string shortDesc)
                where T : IEntity
            {
                m_fields.Add(new EntityField(id, m_id.Field(id, name, shortDesc), (module, entity) => entity is T, 20.ToFix32()));
                return this;
            }

            public Builder AddEntityField(string id, string name, string shortDesc, Func<Module, IEntity, bool> filter = null)
            {
                m_fields.Add(new EntityField(id, m_id.Field(id, name, shortDesc), filter, 20.ToFix32()));
                return this;
            }

            public Builder AddEntityField<T>(string id, string name, string shortDesc, Func<Module, IEntity, bool> filter = null, bool showInTooltip = false)
                where T : IEntity
            {
                m_fields.Add(new EntityField(id, m_id.Field(id, name, shortDesc), (module, entity) => entity is T && (filter?.Invoke(module, entity) ?? true), 20.ToFix32()));
                return this;
            }

            public Builder AddEntityField(Type t, string id, string name, string shortDesc, Func<Module, IEntity, bool> filter = null, bool showInTooltip = false)
            {
                m_fields.Add(new EntityField(id, m_id.Field(id, name, shortDesc), (module, entity) => entity?.GetType()?.IsAssignableTo(t) ?? false && (filter?.Invoke(module, entity) ?? true), 20.ToFix32()));
                return this;
            }

            public Builder AddEntityTypeField<T>(string id, string name, string shortDesc = null, Func<Module, T, bool> filter = null, bool overrideInput = false, bool showInTooltip = false)
                where T : EntityProto, IProtoWithIcon
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new EntityTypeField<T>(id, m_id.Field(id, name, shortDesc ?? ""), filter ?? ((m, proto) => true), showInTooltip));
                return this;
            }

            public Builder AddProductField(string id, string name, string shortDesc = null, Func<Module, ProductProto, bool> filter = null, bool overrideInput = false, bool showInTooltip = false)
            {
                if (overrideInput) {
					addOverrideToggle(id);
				}
				m_fields.Add(new ProductField(id, m_id.Field(id, name, shortDesc ?? ""), filter ?? ((m, proto) => true), showInTooltip));
                return this;
            }

            public Builder AddCustomField(string id, string name, CustomFieldConstructor ui, Action<CustomField> data = null)
            {
                m_fields.Add(new CustomField(id, m_id.Field(id, name), ui, data ?? ((field) => { })));
                return this;
            }

            public Builder AddCustomField(string id, string name, string shortDesc, CustomFieldConstructor ui, Action<CustomField> data = null)
            {
                m_fields.Add(new CustomField(id, m_id.Field(id, name, shortDesc), ui, data ?? ((field) => { })));
                return this;
            }

            public Builder AddCustomField(string id, string name, CustomFieldConstructorWithModule ui, Action<CustomField> data = null)
            {
                m_fields.Add(new CustomField(id, m_id.Field(id, name), ui, data ?? ((field) => { })));
                return this;
            }

            public Builder AddCustomField(string id, string name, string shortDesc, CustomFieldConstructorWithModule ui, Action<CustomField> data = null)
            {
                m_fields.Add(new CustomField(id, m_id.Field(id, name, shortDesc), ui, data ?? ((field) => { })));
                return this;
            }

            public Builder UseComputation(PartialQuantity quantity)
            {
                m_usedComputing = quantity;
                return this;
            }

            /// <summary>
            /// Per-instance computing override.  When set, Controller's
            /// GetRequiredComputation calls this for each module of this proto
            /// instead of using the static UsedComputing.  Use for modules
            /// whose cost depends on per-instance state (e.g. PLC: cost grows
            /// with the parsed lexer-node count).
            /// </summary>
            public Builder UseDynamicComputation(Func<Module, PartialQuantity> calculator)
            {
                m_dynamicComputing = calculator;
                return this;
            }

            public Builder UseMaintenanceT3(int count = 1)
            {
                return UseMaintenance(Ids.Products.MaintenanceT3, count);
            }
        }

        public void ExecuteInit(Module m, bool log = true)
        {
            foreach (var field in Fields)
            {
                field.InitData(m);
            }
            m.SetStatus(Init.Invoke(m));
            if (log) {
				Log.Info($"Module initialized: {m.Id} ({m.Prototype.Id.Value}) with status {m.Status}");
			}
		}
    }

    public static class ModuleProtoExtensions
    {
        public static ModuleProto.Builder ModuleBuilderStart(this ProtoRegistrator registrator, string id, string name, string symbol, string gfx = null)
        {
            return new ModuleProto.Builder(registrator, id, name, "", symbol, new ModuleProto.Gfx(gfx ?? Assets.Base.Products.Icons.Vegetables_svg));
        }

        public static ModuleProto.Builder ModuleBuilderStart(this ProtoRegistrator registrator, string id)
        {
            return new ModuleProto.Builder(registrator, id);
        }

        public static Proto.Str Input(this ModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__input__" + name), text, description);
        }

        public static Proto.Str Output(this ModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__output__" + name), text, description);
        }

        public static Proto.Str Display(this ModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__display__" + name), text, description);
        }

        public static Proto.Str Field(this ModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__field__" + name), text, description);
        }
    }
}
