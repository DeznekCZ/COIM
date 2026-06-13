using System.Collections.Generic;
using Mafi.Collections;

namespace CustomAssets.Editor.Model {

    /// One ingredient or output. Mirrors the `Product(product, quantity, port="*")`
    /// constructor from the CustomAssets Python API. IDs are kept as strings —
    /// modders may reference products by Ids.Products.X (typed) or "Product_X"
    /// (literal), but at the editor's level they're all opaque identifiers.
    public sealed class ProductRef {
        public string ProductId;
        public int Quantity;
        public string Port;       // null or "" means default ("*")

        public ProductRef() { }

        public ProductRef(string productId, int quantity, string port = null) {
            ProductId = productId;
            Quantity = quantity;
            Port = port;
        }
    }

    /// One enrichment step — mirrors the runtime
    /// <c>EnrichmentStepData(fuelMultiplier, breedingRatio, steamReductionDiv)</c>
    /// struct, surfaced as Python <c>EnrichmentStep(...)</c>. Multiple
    /// steps make up a breeding curve the operator advances through;
    /// each step trades fuel efficiency for output (the multiplier),
    /// rate of breeding (the ratio), and steam dampening (the divisor).
    public sealed class EnrichmentStepRef {
        /// FuelMultiplier expressed as a percent value (e.g. <c>100</c>
        /// for 1×, <c>120</c> for 1.2×). Stored as an int to match the
        /// runtime <c>Percent</c> value's integer-percent representation.
        public int FuelMultiplierPercent;
        /// How many output units are bred per input unit. Larger means
        /// more breeding per fuel processed.
        public int BreedingRatio;
        /// Divides the reactor's per-step steam output — higher values
        /// throttle steam more aggressively at this enrichment level.
        public int SteamReductionDiv;

        public EnrichmentStepRef() { }
        public EnrichmentStepRef(int fuelMultiplierPercent, int breedingRatio, int steamReductionDiv) {
            FuelMultiplierPercent = fuelMultiplierPercent;
            BreedingRatio = breedingRatio;
            SteamReductionDiv = steamReductionDiv;
        }
    }

    /// Reactor enrichment / breeding chemistry — mirrors the runtime
    /// <c>EnrichmentData</c> class, surfaced as Python
    /// <c>Enrichment(...)</c>. Held on <see cref="NuclearReactorDef.Enrichment"/>
    /// so a build_nuclear_reactor call can override the source reactor's
    /// breeding setup. Each field doubles as an inherit-from-source
    /// signal when left null/empty.
    public sealed class EnrichmentRef {
        /// ProductProto id of the breeding input (e.g. depleted fuel).
        public string InputProductId;
        /// Port letter the input arrives at on the reactor's layout.
        public string InPort;
        /// ProductProto id of the breeding output (e.g. enriched fuel).
        public string OutputProductId;
        /// Port letter the output is dispensed from.
        public string OutPort;
        /// PartialQuantity — how much input gets converted into output
        /// per processing step. Stored as a numerator / denominator pair
        /// so fractional rates (e.g. 1.2 per step) round-trip exactly.
        /// Null = inherit from source.
        public int? ProcessedPerLevelNumerator;
        public int? ProcessedPerLevelDenominator;
        /// Buffers capacity (raw quantity). Null = inherit.
        public int? BuffersCapacity;
        /// Destroys input + output products on meltdown when true. Null
        /// = inherit from source.
        public bool? DestroyContentOnMeltdown;
        /// Which step is the reactor at on first start. 0-based index
        /// into <see cref="Steps"/>; null = inherit from source.
        public int? DefaultEnrichmentStep;
        /// Enrichment curve — at least one entry required when the modder
        /// supplies <see cref="EnrichmentRef"/>; null/empty inherits the
        /// source reactor's step list.
        public List<EnrichmentStepRef> Steps;

        public EnrichmentRef() { }
    }

    /// One nuclear-reactor fuel cycle — mirrors the
    /// <c>FuelPair(fuelIn, spentFuelOut, durationSeconds)</c> Python
    /// constructor. Each reactor takes a list of these covering every
    /// fuel chemistry it accepts (e.g. uranium → spent uranium,
    /// MOX → spent MOX). Held on <see cref="NuclearReactorDef.FuelPairs"/>
    /// so a build_nuclear_reactor call can substitute its own fuel list
    /// instead of inheriting the source reactor's.
    public sealed class FuelPairRef {
        /// ProductProto id of the fresh fuel rod.
        public string FuelIn;
        /// ProductProto id of the spent fuel pellet returned.
        public string SpentFuelOut;
        /// How long the fuel lasts at power level 1, in seconds.
        public int DurationSeconds;

        public FuelPairRef() { }
        public FuelPairRef(string fuelIn, string spentFuelOut, int durationSeconds) {
            FuelIn = fuelIn;
            SpentFuelOut = spentFuelOut;
            DurationSeconds = durationSeconds;
        }
    }

    /// One I/O port spec — mirrors the `Port(name, type, shape, position,
    /// direction, canOnlyConnectToTransports)` constructor from the Python API.
    /// Consumed by <see cref="EditMachinePortsDef"/> and
    /// <see cref="BuildMachineDef"/> when listing the ports to add to a
    /// machine. The position is stored as raw expression text so tuples
    /// (<c>(2, 0, 0)</c>) and constructors (<c>Vector3i(...)</c>) both
    /// round-trip verbatim.
    public sealed class PortRef {
        /// Single-character port label (the recipe-side selector).
        public string Name;

        /// "input" or "output" (case-insensitive on parse; we keep
        /// whatever the modder wrote so a Title-Case "Input" survives a
        /// round-trip even though the runtime is case-insensitive).
        public string Type;

        /// IoPortShape proto id, e.g. "IoPortShape_Pipe" /
        /// "IoPortShape_FlatConveyor" / "IoPortShape_LooseMaterialConveyor" /
        /// "IoPortShape_MoltenMetalChannel".
        public string Shape;

        /// Parsed X/Y/Z for the common `position = (x, y, z)` / `(x, y)`
        /// tuple case. The loader fills these in directly from the AST
        /// (no string round-trip), and the emitter writes them out as
        /// `(X, Y, Z)` UNLESS <see cref="PositionExpression"/> is set —
        /// in which case the raw expression takes precedence so typed-refs
        /// like <c>Vector3i(...)</c> or <c>Ids.X.Y</c> round-trip verbatim.
        public int PositionX;
        public int PositionY;
        public int PositionZ;

        /// Optional raw expression text. Non-null only when the modder
        /// wrote something the loader couldn't reduce to plain integers
        /// (typed-refs, named constructors). When set, this string is
        /// emitted verbatim; X/Y/Z are ignored on emit.
        public string PositionExpression;

        /// "+X" / "-X" / "+Y" / "-Y".
        public string Direction;

        /// Optional. Defaults to false. Emitted only when true so the file
        /// doesn't accrete explicit-default args.
        public bool CanOnlyConnectToTransports;

        public PortRef() { }
    }

    /// Base class for any top-level definition statement the loader recognises
    /// in a pack file (build_recipe, build_product_*, build_research, add_*,
    /// …). Holds the bookkeeping that's common across every kind of
    /// definition: which file it came from, its source line range, leading
    /// comments, and any wrapping `if/elif/else` clause.
    ///
    /// Concrete subclasses add the typed argument fields. A generic fallback
    /// (<see cref="UnknownDef"/>) captures the call name + raw arguments for
    /// definition kinds the editor doesn't have a typed form for yet — those
    /// still show up in the tree as read-only entries so modders can see
    /// what's in their file even before we wire up a typed editor.
    public abstract class DefBase {
        /// Display kind — "recipe", "product (loose)", "research", … —
        /// surfaced in the tree-group header so modders can scan a file's
        /// definitions at a glance. Concrete subclasses set this in their
        /// constructor or override the property.
        public abstract string Kind { get; }

        /// Polymorphic id surfaced in tree labels, save-restore selection,
        /// dictionary keys, etc. Concrete kinds wire this to their primary
        /// identifier field (recipe id, product id, path, …) so consumers
        /// don't need to downcast. Default returns empty for kinds whose
        /// "identifier" is multi-part (unlocks, edit-recipe by composite
        /// key) — those override to assemble a meaningful display id.
        public virtual string DisplayId => "";

        /// Optional display name. Mirrors <see cref="DisplayId"/>'s role: a
        /// uniform polymorphic accessor so the tree-label code doesn't have
        /// to downcast. Defaults to null — kinds that have a name field
        /// override (typically via the <see cref="NamedDef"/> intermediate).
        public virtual string DisplayName => null;

        /// 1-based source line range, inclusive. PackEmitter splices recipes
        /// by these line numbers; other Def kinds don't round-trip yet so the
        /// numbers are purely for display ("source: file.py (lines N–M)").
        public string SourceFile;
        public int SourceStartLine;
        public int SourceEndLine;

        /// 1-based line of the underlying AST node (the build_*/add_* call's
        /// own StartLine, before any comment-block attachment pulled
        /// SourceStartLine back). Used as the lookup key when the structural
        /// tree walks the AST and needs to find the captured Def for a given
        /// EvaluateStatement — SourceStartLine alone can drift from the
        /// AST's line when a leading `#` comment block is attached.
        public int AstStartLine;

        /// Shared (per-file) map of variable name → resolved underlying id.
        /// Populated by PackLoader from name-binding assignments such as
        /// <c>researchX = build_research(researchId="CustomResearch_X", ...)</c>
        /// — every Def loaded from the same file gets the SAME map instance
        /// by reference, so any later mutation (e.g. when a new assignment
        /// is captured) is visible to all defs in the file.
        ///
        /// Two consumers:
        ///   1. PackEmitter — when emitting an id ref, if the value matches
        ///      a known variable name, emit as a bare identifier instead of
        ///      a quoted string. That's what makes `add_unlock_product(researchX, "Y")`
        ///      round-trip exactly to its original form.
        ///   2. UI pickers — given an id like "researchX" that doesn't
        ///      resolve in ProtosDb, the picker consults this map to find
        ///      the underlying registered id ("CustomResearch_X") and looks
        ///      that up instead. So the form shows the correct proto even
        ///      though the source uses the variable form.
        public System.Collections.Generic.Dictionary<string, string> SourceFileVariables;

        /// `#` comment block immediately above the definition; captured by
        /// the loader and re-emitted in the same place. Recipes use this
        /// already; other Def kinds inherit the field but only display it for
        /// now.
        public string Comment;

        /// Optional Python variable name the def's call was assigned to
        /// — i.e. <c>filter_media_mat = add_loose_product_material(...)</c>
        /// captures <c>"filter_media_mat"</c> here. Null/empty means the
        /// call was a bare expression statement. Round-tripped by the
        /// emitter: when set, the rendered call is prefixed with
        /// <c>&lt;VariableName&gt; = </c> so downstream code that references
        /// the variable (e.g. <c>material = filter_media_mat</c>) still
        /// resolves. Edited in the editor's per-def header alongside the
        /// comment field.
        public string VariableName;

        /// Identifies the statement-list scope this def belongs to within
        /// its source file. Top-level statements share <c>"top"</c>; every
        /// <c>if</c>/<c>elif</c>/<c>else</c> clause body becomes its own
        /// scope keyed as <c>"clause:&lt;headerLine&gt;"</c>. The tree renders
        /// each scope independently and drag-reorder only shuffles defs
        /// within their own scope, so a recipe can be moved up/down within
        /// its clause but not lifted out of it.
        public string ScopeKey;

        /// 0-based index of the contiguous "run" this def belongs to within
        /// its <see cref="ScopeKey"/>. A run is a maximal sequence of
        /// consecutive def-statements at the same scope; any <c>if</c>-
        /// chain in the middle splits the surrounding scope into multiple
        /// runs. Drag-reorder is confined to a single run so structural
        /// clauses stay anchored where the modder wrote them.
        public int RunIndex;

        /// Marks this def as having unsaved changes (field edit OR a
        /// reorder that touched it). The tree row prefixes a "â—" marker
        /// when set; <see cref="PackEmitter.Save"/> clears every Dirty
        /// flag in the affected files once their on-disk form matches the
        /// model again.
        public bool Dirty;

        /// Structured view of this definition's entity layout, when the kind
        /// supports one (see <see cref="ILayoutHostDef"/> and
        /// <see cref="SupportsLayout"/>). Null for kinds that have no layout.
        /// Backfilled by the loader from the parsed <c>layout_str</c> and
        /// driven by the visual layout editor; the emitter re-serialises it
        /// through <see cref="Io.LayoutCodec"/> when the modder has edited it.
        public LayoutModel Layout { get; set; }

        /// Whether this definition kind carries an editable entity layout.
        /// Concrete layout-bearing kinds override to return true; everything
        /// else inherits false so the editor only shows the layout panel where
        /// it makes sense.
        public virtual bool SupportsLayout => false;

        /// Names of mandatory fields whose value is still missing. Returns
        /// an empty list when the def is complete enough to be written to
        /// disk. Per-kind concrete classes override to enumerate their own
        /// required fields (e.g. <c>machineId</c> + <c>source</c> on
        /// <see cref="BuildMachineDef"/>); the default base
        /// implementation declares every def as already-complete so kinds
        /// that don't yet override pass through verbatim.
        ///
        /// Used by the editor to:
        ///   1. SKIP the immediate <c>PackEmitter.AppendDef</c> call on
        ///      "+ add" so a freshly-added def stays in-memory only until
        ///      the modder has filled the required fields.
        ///   2. BLOCK <c>onSaveDef</c> / <c>onSavePack</c> from writing an
        ///      incomplete new def, with a Log.Warning surfaced to the
        ///      modder so they know what's still missing.
        public virtual Lyst<string> MissingMandatoryFields() {
            return new Lyst<string>();
        }
    }

    /// Intermediate base for kinds that have both a primary id AND a
    /// human-facing display name (recipes, research nodes, products,
    /// generators, toolbar categories). Kinds without a separate display
    /// name (textures, materials, prefabs, unlocks) inherit
    /// <see cref="DefBase"/> directly and define their own id field.
    public abstract class NamedDef : DefBase {
        /// Stable identifier surfaced as the primary <c>id</c>-style
        /// argument on the underlying Python call (recipeId, productId,
        /// researchId, …). Concrete subclasses typically expose an
        /// alias property with the kind's API-canonical name.
        public string Id;

        /// Localized display name shown next to the id in the tree label,
        /// editor header, etc. Optional — falls back to <see cref="Id"/>
        /// when null/empty.
        public string Name;

        public override string DisplayId => Id;
        public override string DisplayName => Name;
    }

    /// Wrapper for a top-level <c>if/elif/else</c> clause that nests other
    /// defs. The condition string is meaningful only here — individual defs
    /// inside the block don't carry their own copy. The tree builder walks
    /// the AST and creates a <see cref="IfBlockDef"/> per clause; the
    /// emitter / save flow stays line-range-based so this class is purely a
    /// structural marker for the UI, not part of the round-trip pipeline.
    public sealed class IfBlockDef : DefBase {
        public override string Kind => "if";
        public override string DisplayId => Condition;

        /// The clause's condition expression (e.g.
        /// <c>product_exist("Product_X")</c>). Empty/null for an
        /// <c>else</c> clause.
        public string Condition;
    }

    /// Catch-all for definition calls the editor doesn't have a typed model
    /// for yet (build_product_loose, build_research, add_texture, etc.). The
    /// loader still surfaces them in the tree so modders see the file's full
    /// contents at a glance; clicking one shows a read-only form with the
    /// captured callee name + source range. Editing a typed form lands when
    /// the corresponding concrete Def subclass arrives.
    public sealed class UnknownDef : DefBase {
        /// The Python function being called — "build_product_loose",
        /// "add_texture", etc. Used for the tree label and the read-only
        /// header in the editor.
        public string CallName;

        /// First positional id-shaped argument the loader saw, if any —
        /// gives the tree row a more specific label than just the call
        /// name. Null for calls with no obvious identifier.
        public string CapturedId;

        /// Verbatim source text of the call (joined with '\n'). Edited
        /// directly in the editor's multiline form when the user picks the
        /// definition — round-trip works because PackEmitter splices this
        /// string back at the original line range without any typed parsing.
        /// Until typed editors land for each kind (product, research, asset…)
        /// this raw-edit path is what lets modders update those definitions
        /// without leaving the in-game editor.
        public string RawSource;

        public override string Kind => CallName ?? "definition";
        public override string DisplayId =>
            !string.IsNullOrEmpty(CapturedId) ? CapturedId : (CallName ?? "");
    }

    /// `add_unlock_recipe(research, machine, proto)` — adds a recipe to an
    /// existing research node. Three id arguments, all required.
    public sealed class UnlockRecipeDef : DefBase {
        public override string Kind => "unlock-recipe";
        public string ResearchId;
        public string MachineId;
        public string RecipeId;
        // Composite display key — the tree label uses this so unlock rows
        // distinguish themselves at a glance even though there's no single
        // "id" field.
        public override string DisplayId =>
            (RecipeId ?? "?") + " @ " + (MachineId ?? "?");
    }

    /// `add_unlock_product(research, product)` — adds a product to an existing
    /// research node. Two id arguments, both required.
    public sealed class UnlockProductDef : DefBase {
        public override string Kind => "unlock-product";
        public string ResearchId;
        public string ProductId;
        public override string DisplayId => ProductId ?? "";
    }

    /// `add_unlock_machine(research, machine)` — adds a machine to an existing
    /// research node. Two id arguments, both required.
    public sealed class UnlockMachineDef : DefBase {
        public override string Kind => "unlock-machine";
        public string ResearchId;
        public string MachineId;
        public override string DisplayId => MachineId ?? "";
    }

    /// `build_research(researchId, name, description, costs, position, icon)`
    /// — registers a new research node. Modders later wire products / recipes /
    /// machines to it via `add_unlock_*`.
    public sealed class ResearchDef : NamedDef {
        public override string Kind => "research";

        /// Alias for the inherited <see cref="NamedDef.Id"/>, matching the
        /// Python argument name `researchId`.
        public string ResearchId { get => Id; set => Id = value; }

        public string Description;

        /// Cost — int or ResearchCostsTpl in the source. We capture the raw
        /// expression text so it can survive a round-trip when it's a typed
        /// reference like `ResearchCostsTpl.Tier2()`. Plain ints get parsed
        /// and shown in the form as a number; non-ints fall back to a raw
        /// text edit.
        public string CostsExpression;
        public int? CostsAsInt;

        public int? PositionX;
        public int? PositionY;
        public string IconPath;

        /// Minimum-tier research lab requirement, expressed as the id of the
        /// research-pack product the lab consumes (LabEquipment / LabEquipment2
        /// / LabEquipment3 / LabEquipment4 in the vanilla set, plus any modded
        /// research-pack products). Optional — null means "no tier hint", which
        /// lets the framework pick the basic lab. Stored as the bare product
        /// id; the emitter writes it as a typed-ref when one is registered
        /// (Ids.Products.X) or a plain string otherwise.
        public string TierProductId;
    }

    /// Common base for the three `build_product_*` variants. Holds the
    /// shared arguments — id, name, description, icon, isStorable, isWaste,
    /// research — so per-kind subclasses only add what's specific to their
    /// product shape (material + dumping for loose, fluid colors for fluid,
    /// prefab + packing for unit).
    public abstract class ProductDefBase : NamedDef {
        /// Alias for the inherited <see cref="NamedDef.Id"/>, matching the
        /// Python `productId` arg.
        public string ProductId { get => Id; set => Id = value; }

        public string Description;
        public string IconPath;

        public bool IsStorable;
        public bool IsWaste;

        /// Research that unlocks this product. Optional; null = unlocked by
        /// default (no research gate). Stored as an id string so it round-
        /// trips through the typed-ref / string-literal heuristic.
        public string ResearchId;
    }

    /// `build_product_loose(...)` — pile-style product (sand, ores, etc.).
    /// Adds the material + appearance + dumping fields on top of the base.
    public sealed class ProductLooseDef : ProductDefBase {
        public override string Kind => "product (loose)";

        /// Material reference — typed-ref like `Assets.X.Y_mat`, a string
        /// literal asset path, or a name binding to an add_*_material()
        /// result. Captured as raw expression text so all three shapes
        /// round-trip verbatim.
        public string MaterialExpression;

        /// RGB triples are stored as raw expression text so tuples like
        /// `(170, 165, 155)` survive a round-trip without lossy parsing.
        public string ColorExpression;
        public string ParticleColorExpression;

        public bool IsDumped;
        public bool IsRecyclable;
        public bool IsRough;
        public bool PinToHomeScreen;
        public int? MaxTransport;
        public string PrefabPath;

        /// Terrain-material id this product dumps as (e.g. Ids.TerrainMaterials.Gravel).
        public string DumpsAsId;
    }

    /// `build_product_fluid(...)` — pipe-routed product (water, oil, gas).
    public sealed class ProductFluidDef : ProductDefBase {
        public override string Kind => "product (fluid)";

        public string ColorExpression;
        public string TransportColorExpression;
        public string TransportAccentColorExpression;
        public bool CanBeDiscarded = true;
    }

    /// `build_product_unit(...)` — countable item on conveyor (machinery
    /// parts, batteries, etc.).
    public sealed class ProductUnitDef : ProductDefBase {
        public override string Kind => "product (unit)";

        /// Prefab reference. Same expression-text capture as material.
        public string PrefabExpression;

        public int? MaxTransport;

        /// Stacking mode string (e.g. "Auto", "Triangle", "Row").
        /// Captured verbatim — the runtime accepts both case-insensitive
        /// strings and enum values, but our emit always uses a string
        /// literal so it round-trips identically.
        public string PackingMode;

        public bool AllowPackingNoise;
        public bool RotateSecondPackedItem90Degs;
    }

    /// `add_texture(path, replace=None)` — registers a texture asset.
    /// `path` is the in-pack relative path; `replace` is optional and points
    /// at the vanilla asset path the new texture overrides.
    public sealed class TextureDef : DefBase {
        public override string Kind => "texture";

        /// `add_texture`'s `path` arg — the asset's id in the registry.
        public string Path;
        public string ReplacePath;

        public override string DisplayId => Path ?? "";
    }

    /// `add_loose_product_material(path, albedo, normals=None, metallic=None,
    /// reference=None, tiling=1)` — registers a pile-material asset for a
    /// loose product. Texture inputs (`albedo`, `normals`, `metallic`) can
    /// each be a single path/Tex or a list; we capture them as raw
    /// expression strings so list forms round-trip verbatim.
    public sealed class MaterialLooseDef : DefBase {
        public override string Kind => "material (loose)";

        public string Path;
        public string AlbedoExpression;
        public string NormalsExpression;
        public string MetallicExpression;
        public string ReferenceExpression;
        public string TilingExpression;

        public override string DisplayId => Path ?? "";
    }

    /// `add_prefab_box(path, texture=None)` — simple cuboid prefab. Used as
    /// the prefab arg of build_product_unit for boxy items.
    public sealed class PrefabBoxDef : DefBase {
        public override string Kind => "prefab (box)";

        public string Path;
        public string TextureExpression;

        public override string DisplayId => Path ?? "";
    }

    /// `add_unit_prefab(path, albedo, normals=None, metallic=None,
    /// reference=None, width=0.5, height=0.2, depth=0.5, mesh=None)` —
    /// registers a single-GameObject prefab carrying one mesh + one material
    /// for use as the `prefab` arg on build_product_unit(). Texture channels
    /// follow the same multi-shape (path / Tex / list) convention as
    /// add_loose_product_material, captured as raw expressions for lossless
    /// round-trip. Dimensions default to a small box in the API (0.5/0.2/0.5)
    /// so we keep them nullable to distinguish "omitted" from "explicit".
    public sealed class UnitPrefabDef : DefBase {
        public override string Kind => "prefab (unit)";

        public string Path;
        public string AlbedoExpression;
        public string NormalsExpression;
        public string MetallicExpression;
        public string ReferenceExpression;
        public double? Width;
        public double? Height;
        public double? Depth;
        public string MeshPath;
        /// Winding mode for the loaded .obj. `null` (omitted) and `"ccw"` both mean
        /// pass-through (standard OBJ); `"cw"` reverses fan-triangulation. Stored as
        /// a string so the value round-trips verbatim through the editor and emitter.
        public string Winding;

        public override string DisplayId => Path ?? "";
    }

    /// `add_texture_material(path, texture=None, reference=None, shader=None)` —
    /// registers a Material asset. Plain texture (no channel split) + clone
    /// source + optional shader override. Use for non-loose materials (unit
    /// prefabs, decorative surfaces).
    public sealed class MaterialTextureDef : DefBase {
        public override string Kind => "material (texture)";

        public string Path;
        public string TextureExpression;
        public string ReferenceExpression;
        public string Shader;

        public override string DisplayId => Path ?? "";
    }

    /// `edit_recipe(recipe, duration, ingredients, products, machine,
    /// research, power)` — mutates an existing RecipeProto (vanilla or
    /// modded) in place. Same field surface as build_recipe minus
    /// id/name/description (which are immutable on an existing recipe). Each
    /// optional arg, when omitted, leaves the corresponding field unchanged
    /// at runtime — we mirror that by keeping null = "argument was omitted".
    public sealed class EditRecipeDef : DefBase {
        public override string Kind => "edit-recipe";

        public string RecipeId;

        public int? DurationSeconds;
        public List<ProductRef> Ingredients;
        public List<ProductRef> Products;
        public string MachineId;
        public string ResearchId;
        public int? PowerPercent;

        public override string DisplayId => RecipeId ?? "";
    }

    /// `edit_machine_ports(machine, add_ports=[Port(...), ...])` — appends
    /// ports to an existing machine layout without changing footprint,
    /// recipes, or graphics. Round-trips through PackLoader/Emitter
    /// alongside the typed Port refs.
    public sealed class EditMachinePortsDef : DefBase {
        public override string Kind => "edit-machine-ports";

        /// Target machine's id. Vanilla machines (string) or typed-refs
        /// (Ids.Machines.X) both supported via the appendIdRef heuristic.
        public string MachineId;

        public List<PortRef> AddPorts = new List<PortRef>();

        public override string DisplayId => MachineId ?? "";

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(MachineId)) missing.Add("machine");
            return missing;
        }
    }

    /// `build_machine(machineId, source, name, description, ports,
    /// consumedPowerPerTick, research, copy_recipes, copy_layout,
    /// copy_ports, copy_graphics, lockedOnInit)` — sibling of
    /// build_generator. Creates a new MachineProto by cloning a source
    /// machine and applying overrides; the <c>copy_*</c> flags select
    /// which source fields are inherited vs. blank.
    public sealed class BuildMachineDef : NamedDef, ILayoutHostDef {
        public override string Kind => "build-machine";
        public override bool SupportsLayout => true;

        public string MachineId { get => Id; set => Id = value; }

        public string SourceId;
        public string Description;
        public List<PortRef> AddPorts = new List<PortRef>();

        /// Optional override for the machine's footprint layout. Machines
        /// normally inherit their tile shape from the source via
        /// <see cref="CopyLayout"/>; supplying a layout string (authored in
        /// the visual editor) REPLACES that shape. When set the emitter also
        /// writes <c>copy_layout = False</c> so the authored layout wins.
        public string LayoutSourceStr { get; set; }

        /// <see cref="ILayoutHostDef"/> — machines route their port list to
        /// <see cref="AddPorts"/>.
        public List<PortRef> Ports => AddPorts ?? (AddPorts = new List<PortRef>());
        public int? ConsumedPowerPerTickKw;
        public string ResearchId;
        public bool CopyRecipes = true;
        public bool? LockedOnInit;

        /// Mirror of the runtime's `copy_layout` arg. When false the new
        /// machine starts from a 1×1 empty layout instead of cloning the
        /// source's tile shape; the modder is expected to define its ports
        /// via <see cref="AddPorts"/>. Defaults to true to match the
        /// original "clone everything" contract.
        public bool CopyLayout = true;

        /// Mirror of the runtime's `copy_ports` arg. When false the new
        /// machine doesn't inherit any of the source's existing ports — the
        /// modder is expected to populate <see cref="AddPorts"/> with the
        /// complete port list (typically by clicking the editor's
        /// "Import source ports" button to start from the source's set
        /// and then trimming / modifying it).
        public bool CopyPorts = true;

        /// Mirror of the runtime's `copy_graphics` arg. When false the
        /// new machine uses <c>MachineProto.Gfx.Empty</c> instead of the
        /// source's prefab / sounds / particles. Modders typically pair
        /// this with a custom prefab supplied via a follow-up step.
        public bool CopyGraphics = true;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(MachineId)) missing.Add("machineId");
            if (string.IsNullOrEmpty(SourceId))  missing.Add("source");
            return missing;
        }
    }

    /// `build_housing(housingId, source, name, description, capacity,
    /// upointsCapacity, research, lockedOnInit)` — clones an existing
    /// SettlementHousingModuleProto under a new id with optional
    /// modder-facing overrides (capacity, unity-points capacity).
    /// Inherits layout / graphics / costs / unity-needs profile from
    /// the source.
    public sealed class HousingDef : NamedDef, ILayoutHostDef {
        public override string Kind => "housing";
        public override bool SupportsLayout => true;

        /// <see cref="ILayoutHostDef"/> — settlement housing has no I/O ports.
        public List<PortRef> Ports => null;

        public string HousingId { get => Id; set => Id = value; }

        public string SourceId;
        public string Description;

        public int? Capacity;
        public int? UpointsCapacity;

        /// Optional override for the source's layout string. See the
        /// matching field on [[NuclearReactorDef]] for the full rationale —
        /// non-null = REPLACE the source's layout entirely; the editor
        /// pre-populates this from the picked source's SourceLayoutStr
        /// via the Fill-from-source button.
        public string LayoutSourceStr { get; set; }

        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(HousingId)) missing.Add("housingId");
            if (string.IsNullOrEmpty(SourceId))  missing.Add("source");
            return missing;
        }
    }

    /// `build_settlement_decoration(decorationId, source, name, description,
    /// upointsBonus, bonusRange, research, lockedOnInit)`.
    public sealed class SettlementDecorationDef : NamedDef, ILayoutHostDef {
        public override string Kind => "settlement-decoration";
        public override bool SupportsLayout => true;
        public List<PortRef> Ports => null;
        public string DecorationId { get => Id; set => Id = value; }
        public string SourceId;
        public string Description;
        public int? UpointsBonus;
        public int? BonusRange;
        public string LayoutSourceStr { get; set; }
        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(DecorationId)) missing.Add("decorationId");
            if (string.IsNullOrEmpty(SourceId))     missing.Add("source");
            return missing;
        }
    }

    /// `build_settlement_food(foodModuleId, source, name, description,
    /// buffersCount, capacityPerBuffer, research, lockedOnInit)`.
    public sealed class SettlementFoodDef : NamedDef, ILayoutHostDef {
        public override string Kind => "settlement-food";
        public override bool SupportsLayout => true;
        public List<PortRef> Ports => null;
        public string FoodModuleId { get => Id; set => Id = value; }
        public string SourceId;
        public string Description;
        public int? BuffersCount;
        public int? CapacityPerBuffer;
        public string LayoutSourceStr { get; set; }
        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(FoodModuleId)) missing.Add("foodModuleId");
            if (string.IsNullOrEmpty(SourceId))     missing.Add("source");
            return missing;
        }
    }

    /// `build_settlement_isp(ispModuleId, source, name, description,
    /// computingPer100Pops, electricityConsumedKw, research, lockedOnInit)`.
    public sealed class SettlementIspDef : NamedDef, ILayoutHostDef {
        public override string Kind => "settlement-isp";
        public override bool SupportsLayout => true;
        public List<PortRef> Ports => null;
        public string IspModuleId { get => Id; set => Id = value; }
        public string SourceId;
        public string Description;
        public int? ComputingPer100Pops;
        public int? ElectricityConsumedKw;
        public string LayoutSourceStr { get; set; }
        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(IspModuleId)) missing.Add("ispModuleId");
            if (string.IsNullOrEmpty(SourceId))    missing.Add("source");
            return missing;
        }
    }

    /// `build_hospital(hospitalId, source, name, description, powerRequiredKw,
    /// buffersCount, capacityPerBuffer, suppliesPerHundredPopsPerMonth,
    /// research, lockedOnInit)`.
    public sealed class HospitalDef : NamedDef, ILayoutHostDef {
        public override string Kind => "hospital";
        public override bool SupportsLayout => true;
        public List<PortRef> Ports => null;
        public string HospitalId { get => Id; set => Id = value; }
        public string SourceId;
        public string Description;
        public int? PowerRequiredKw;
        public int? BuffersCount;
        public int? CapacityPerBuffer;
        public int? SuppliesPerHundredPopsPerMonth;
        public string LayoutSourceStr { get; set; }
        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(HospitalId)) missing.Add("hospitalId");
            if (string.IsNullOrEmpty(SourceId))   missing.Add("source");
            return missing;
        }
    }

    /// `build_mine_tower(mineTowerId, source, name, description,
    /// research, lockedOnInit)`.
    public sealed class MineTowerDef : NamedDef, ILayoutHostDef {
        public override string Kind => "mine-tower";
        public override bool SupportsLayout => true;
        public List<PortRef> Ports => null;
        public string MineTowerId { get => Id; set => Id = value; }
        public string SourceId;
        public string Description;
        public string LayoutSourceStr { get; set; }
        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(MineTowerId)) missing.Add("mineTowerId");
            if (string.IsNullOrEmpty(SourceId))    missing.Add("source");
            return missing;
        }
    }

    /// `build_research_lab(researchLabId, source, name, description,
    /// electricityConsumedKw, computingConsumed, durationForRecipeSeconds,
    /// sciencePerRecipe, unityMonthlyCost, research, lockedOnInit)`.
    public sealed class ResearchLabDef : NamedDef, ILayoutHostDef {
        public override string Kind => "research-lab";
        public override bool SupportsLayout => true;
        /// <see cref="ILayoutHostDef"/> — labs route ports to
        /// <see cref="AddPorts"/>, lazily allocating the list on demand.
        public List<PortRef> Ports => AddPorts ?? (AddPorts = new List<PortRef>());
        public string ResearchLabId { get => Id; set => Id = value; }
        public string SourceId;
        public string Description;
        public int? ElectricityConsumedKw;
        public int? ComputingConsumed;
        public int? DurationForRecipeSeconds;
        public int? SciencePerRecipe;
        public int? UnityMonthlyCost;
        /// Research labs DO accept input/output products via ports, so
        /// `add_ports` is exposed here. Settlement modules / mine towers
        /// don't use ports in their layouts so they skip this field.
        public List<PortRef> AddPorts;
        public string LayoutSourceStr { get; set; }
        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(ResearchLabId)) missing.Add("researchLabId");
            if (string.IsNullOrEmpty(SourceId))      missing.Add("source");
            return missing;
        }
    }

    /// `build_nuclear_reactor(reactorId, source, name, description,
    /// maxPowerLevel, fuelCapacity, minFuelToOperate, processDurationSeconds,
    /// computingConsumed, research, lockedOnInit)`.
    /// `edit_nuclear_reactor_fuels(reactor, add_fuels=[FuelPair(...)])` —
    /// append fuel cycles to an existing NuclearReactorProto without
    /// cloning it. Useful for mods that want to drop a new fuel rod into
    /// the vanilla reactor (e.g. CANDU rods into the base NuclearReactor)
    /// without redefining every other field. Same FuelPair shape as
    /// build_nuclear_reactor's <c>fuel_pairs</c>.
    ///
    /// Note: the reactor's fuel-in port shape is determined by the FIRST
    /// fuel pair in the list (asserted in NuclearReactorProto's ctor).
    /// Adding fuels of a DIFFERENT product type than the existing first
    /// won't change the port shape — the new fuel can be listed but the
    /// physical port still only accepts the original type. Use
    /// build_nuclear_reactor with explicit port shapes if a different
    /// type is needed.
    public sealed class EditNuclearReactorFuelsDef : DefBase {
        public override string Kind => "edit-nuclear-reactor-fuels";

        public string ReactorId;
        public List<FuelPairRef> AddFuels = new List<FuelPairRef>();

        public override string DisplayId => ReactorId ?? "";

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(ReactorId)) missing.Add("reactor");
            if (AddFuels == null || AddFuels.Count == 0) missing.Add("add_fuels");
            return missing;
        }
    }

    public sealed class NuclearReactorDef : NamedDef, ILayoutHostDef {
        public override string Kind => "nuclear-reactor";
        public override bool SupportsLayout => true;
        /// <see cref="ILayoutHostDef"/> — reactors route ports to
        /// <see cref="AddPorts"/>, lazily allocating the list on demand.
        public List<PortRef> Ports => AddPorts ?? (AddPorts = new List<PortRef>());
        public string ReactorId { get => Id; set => Id = value; }
        public string SourceId;
        public string Description;
        public int? MaxPowerLevel;
        public int? FuelCapacity;
        public int? MinFuelToOperate;
        public int? ProcessDurationSeconds;
        public int? ComputingConsumed;

        /// Optional override for the reactor's fuel cycle list. When null
        /// the build_nuclear_reactor C# ctor keeps the source reactor's
        /// FuelPairs verbatim; when non-empty each entry is converted to
        /// a runtime FuelData(fuelIn, spentFuelOut, duration) struct.
        /// Empty list means "no fuels" which a sane reactor never wants,
        /// so the loader / emitter treat empty == null.
        public List<FuelPairRef> FuelPairs;

        /// Optional IoPortShape proto ids that replace the source
        /// reactor's fuel-in / fuel-out port shapes on the cloned layout.
        /// Useful when the modder's fuel chemistry is a different product
        /// TYPE than the source's. Null = let the runtime infer from the
        /// first fuel pair's product type.
        public string FuelInPortShape;
        public string FuelOutPortShape;

        /// Extra ports to append to the source reactor's layout, on top
        /// of the existing fuel/water/steam/coolant ports. Mirrors the
        /// same field on clone_machine — same PortRef shape, same
        /// add_ports Python arg name. Useful for adding visual / aux
        /// ports beyond the reactor's built-in slots.
        public List<PortRef> AddPorts;

        /// Optional override for the source reactor's SourceLayoutStr.
        /// When non-null + non-empty, REPLACES the source's layout
        /// string entirely (with the same LayoutParams). The editor
        /// pre-populates this from src.SourceLayoutStr so the modder
        /// starts with an editable copy and adjusts tiles / heights /
        /// port positions to match a replaced prefab / texture.
        public string LayoutSourceStr { get; set; }

        // ---- Fluid overrides ------------------------------------------
        // All fluid fields default null → inherit the source reactor's
        // value. The emitter omits null/empty values; the runtime ctor
        // falls back to source.<field> when the override isn't supplied.

        /// ProductProto id of the coolant flowing into the reactor.
        public string CoolantInId;
        /// ProductProto id of the coolant flowing out (typically a
        /// heated variant or different product entirely).
        public string CoolantOutId;
        /// Port letter the coolant-in arrives at. Null = inherit.
        public string CoolantInPort;
        /// Port letter the coolant-out is dispensed from. Null = inherit.
        public string CoolantOutPort;
        /// IoPortShape proto ids overriding the source reactor's coolant
        /// port shapes (paired with the fuel-port-shape overrides). Null
        /// = let the runtime infer from the coolant product's type.
        public string CoolantInPortShape;
        public string CoolantOutPortShape;

        /// Water-in product id + per-power-level quantity (mirrors the
        /// runtime's <c>WaterInPerPowerLevel</c> ProductQuantity). Either
        /// half being null = inherit from source.
        public string WaterInProductId;
        public int? WaterInQuantity;
        /// Steam-out product id + per-power-level quantity. Same rules.
        public string SteamOutProductId;
        public int? SteamOutQuantity;
        /// Comma-separated port letters for water-in and steam-out (e.g.
        /// <c>"WX"</c> for ports W and X). Null = inherit from source.
        public string WaterInPorts;
        public string SteamOutPorts;

        // ---- Enrichment / breeding override --------------------------

        /// Override the source reactor's breeding chemistry. Null =
        /// inherit (the cloned reactor breeds as the source does, or
        /// not at all if the source had no Enrichment). When set, every
        /// non-null field on the <see cref="EnrichmentRef"/> patches the
        /// corresponding runtime field; null sub-fields inherit from the
        /// source's Enrichment so a modder can tweak e.g. just the step
        /// list without re-specifying products + ports.
        public EnrichmentRef Enrichment;

        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(ReactorId)) missing.Add("reactorId");
            if (string.IsNullOrEmpty(SourceId))  missing.Add("source");
            return missing;
        }
    }

    /// `edit_nuclear_reactor_ports(reactor, add_ports=[Port(...), ...])`
    /// — reactor-typed sibling of <see cref="EditMachinePortsDef"/>. Appends
    /// new ports to an existing NuclearReactorProto's layout — typical use
    /// is adding the enrichment in/out ports (or any auxiliary connection)
    /// to a vanilla / modded reactor without cloning it.
    public sealed class EditNuclearReactorPortsDef : DefBase {
        public override string Kind => "edit-nuclear-reactor-ports";

        public string ReactorId;
        public List<PortRef> AddPorts = new List<PortRef>();

        public override string DisplayId => ReactorId ?? "";

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(ReactorId)) missing.Add("reactor");
            return missing;
        }
    }

    /// `edit_nuclear_reactor_fluids(reactor, coolantIn, coolantOut, ...)` —
    /// patch the coolant / water / steam fluids on an existing
    /// NuclearReactorProto without cloning it. Same nullable-inherit
    /// semantics as <see cref="NuclearReactorDef"/>'s fluid block: every
    /// field defaults null → leave the target reactor's value alone.
    public sealed class EditNuclearReactorFluidsDef : DefBase {
        public override string Kind => "edit-nuclear-reactor-fluids";

        public string ReactorId;
        public string CoolantInId;
        public string CoolantOutId;
        public string CoolantInPort;
        public string CoolantOutPort;
        public string CoolantInPortShape;
        public string CoolantOutPortShape;
        public string WaterInProductId;
        public int? WaterInQuantity;
        public string SteamOutProductId;
        public int? SteamOutQuantity;
        public string WaterInPorts;
        public string SteamOutPorts;

        public override string DisplayId => ReactorId ?? "";

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(ReactorId)) missing.Add("reactor");
            // Every override is optional; the validation here is just
            // "is there a target reactor?". The runtime warn-and-no-op
            // when no fields are actually overridden.
            return missing;
        }
    }

    /// `edit_nuclear_reactor_enrichment(reactor, enrichment=Enrichment(...))`
    /// — patch the breeding chemistry on an existing NuclearReactorProto.
    /// The <see cref="Enrichment"/> ref follows the same nullable-inherit
    /// rules as <see cref="NuclearReactorDef.Enrichment"/>: null sub-fields
    /// keep the target's existing value.
    public sealed class EditNuclearReactorEnrichmentDef : DefBase {
        public override string Kind => "edit-nuclear-reactor-enrichment";

        public string ReactorId;
        public EnrichmentRef Enrichment;

        public override string DisplayId => ReactorId ?? "";

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(ReactorId)) missing.Add("reactor");
            if (Enrichment == null) missing.Add("enrichment");
            return missing;
        }
    }

    /// `add_toolbar_category(categoryId, name, icon, parent, entities)` —
    /// registers a new category in the build menu and assigns entities to it.
    public sealed class ToolbarCategoryDef : NamedDef {
        public override string Kind => "toolbar category";

        /// Alias for the inherited <see cref="NamedDef.Id"/>, matching the
        /// Python `categoryId` arg.
        public string CategoryId { get => Id; set => Id = value; }

        public string IconPath;
        public string ParentId;

        /// Raw expression for the `entities` list — `[id1, id2, …]` with
        /// each entry a typed-ref or string literal. Captured verbatim so
        /// list contents (especially typed refs) round-trip exactly.
        public string EntitiesExpression;
    }

    /// `build_generator(id, name, inputProduct, outputElectricityKw,
    /// outputProduct=None, description, source, duration, generationPriority,
    /// bufferCapacityMultiplier, research, lockedOnInit)` — adds an
    /// electricity generator that consumes a product and emits electricity
    /// (plus optional waste byproduct).
    public sealed class GeneratorDef : NamedDef {
        public override string Kind => "generator";

        /// Alias for the inherited <see cref="NamedDef.Id"/>.
        public string GeneratorId { get => Id; set => Id = value; }

        public string Description;

        /// Input/output products are `Product(id, qty, port="*")` calls —
        /// we capture them as raw expression text so the editor's typed
        /// fields are simpler and the round-trip stays lossless. A future
        /// pass can switch these to typed ProductRef values backed by the
        /// game's Product picker.
        public string InputProductExpression;
        public string OutputProductExpression;

        public int? OutputElectricityKw;

        /// Source generator id to clone non-customisable fields from.
        /// Defaults to "DieselGeneratorT2" in the API.
        public string SourceId;

        public int? DurationSeconds;
        public int? GenerationPriority;
        public int? BufferCapacityMultiplier;
        public string ResearchId;
        public bool? LockedOnInit;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(GeneratorId)) missing.Add("id");
            // inputProduct is required by the runtime ctor; an empty
            // expression here means the modder hasn't authored it yet,
            // and emitting `inputProduct = None` crashes the loader on
            // next pack reload (see the matching guard added to
            // edit_machine_ports / clone_machine).
            if (string.IsNullOrEmpty(InputProductExpression)) missing.Add("inputProduct");
            // outputElectricityKw must be positive — 0 = uninitialized draft.
            if (!OutputElectricityKw.HasValue || OutputElectricityKw.Value <= 0)
                missing.Add("outputElectricityKw");
            return missing;
        }
    }

    /// One recipe — mirrors the `build_recipe(...)` API surface in
    /// src/Recipes/CustomAssets/__init__.py. Optional fields use null to mean
    /// "argument was omitted in the source"; the emitter must reproduce this
    /// distinction (omitted vs explicit-default) faithfully so the file stays stable.
    public sealed class RecipeDef : NamedDef {
        public override string Kind => "recipe";

        /// Alias for the inherited <see cref="NamedDef.Id"/>. The existing
        /// editor code reads + writes `RecipeId` extensively; this property
        /// keeps every call site working without churn while letting
        /// NamedDef own the actual storage.
        public string RecipeId { get => Id; set => Id = value; }

        // Recipe-specific arguments.
        public string Description;
        public string MachineId;

        // Optional arguments. Null = not present in source.
        public string ResearchId;
        public int? DurationSeconds;
        public List<ProductRef> Ingredients = new List<ProductRef>();
        public List<ProductRef> Products    = new List<ProductRef>();
        public int? PowerPercent;

        // Note: Name, SourceFile, SourceStartLine, SourceEndLine, Comment, and
        // Condition now live on DefBase — see that class for documentation.
    }

    /// `define_box_type(boxTypeId, token, heightFrom, heightTo, constraint,
    /// surface, terrainMaterial, isRamp)` / alias `layout_token(...)` —
    /// registers a reusable custom tile ("box") type the layout editor offers
    /// in its palette and the runtime feeds to COI's layout parser as a
    /// <c>CustomLayoutToken</c>. The <see cref="Token"/> is the literal
    /// 3-character grid token; a '0' in its middle slot is a height wildcard
    /// (digit 1..9 picked per-tile). The token's first char must not collide
    /// with a port letter (A–Z) or a port-direction arrow (^ &gt; v &lt; +).
    public sealed class BoxTypeDef : NamedDef {
        public override string Kind => "box-type";

        /// Alias for the inherited <see cref="NamedDef.Id"/>, matching the
        /// Python `boxTypeId` argument.
        public string BoxTypeId { get => Id; set => Id = value; }

        /// The literal 3-character layout token (e.g. "=0=" or "<#>"). A '0'
        /// in the middle slot is the per-tile height wildcard.
        public string Token;

        /// Start height in tiles (0 = ground). Maps to
        /// <c>LayoutTokenSpec.heightFrom</c>.
        public int HeightFrom;

        /// End height (exclusive). Null = derive from the per-tile wildcard
        /// digit; otherwise a fixed ceiling regardless of the token digit.
        public int? HeightTo;

        /// Tile constraint name — "None" / "Ground" / "Ocean" (matches COI's
        /// LayoutTileConstraint members). Null/empty = None.
        public string Constraint;

        /// Optional terrain tile surface proto id placed on the tile.
        public string SurfaceId;

        /// Optional terrain material proto id applied to the tile.
        public string TerrainMaterialId;

        /// Whether this token is a vehicle ramp.
        public bool IsRamp;

        public override Lyst<string> MissingMandatoryFields() {
            var missing = new Lyst<string>();
            if (string.IsNullOrEmpty(BoxTypeId)) missing.Add("boxTypeId");
            if (string.IsNullOrEmpty(Token) || Token.Length != 3) missing.Add("token");
            return missing;
        }
    }

    /// One validation issue surfaced by <see cref="Io.PackValidator"/>
    /// after a pack is loaded. Currently flags use-before-definition of
    /// Python variables — referencing <c>filter_media_mat</c> earlier in
    /// the file than the line that assigns it — which the COI runtime
    /// would otherwise fail with a NameError on next pack load.
    public sealed class PackIssue {
        public string SourceFile;
        public int Line;
        public string Message;
    }

    /// One pack worth of recipes the editor has loaded. Keyed off PackRegistry's
    /// LoadedPack — same ModId, same RootPath. Recipes are flat across the pack's
    /// files; SourceFile on each RecipeDef preserves the file the user originally
    /// put it in so saving doesn't shuffle definitions between files.
    public sealed class PackModel {
        public string ModId;
        public string RootPath;

        /// Every top-level definition statement the loader saw — recipes,
        /// product builders, research, asset registrations, etc. Concrete
        /// subclasses cover the kinds with typed editors; the rest fall back
        /// to <see cref="UnknownDef"/>. Surfaced in the tree so modders can
        /// see the full file contents at a glance even before all
        /// definition kinds have editable forms.
        public List<DefBase> Definitions = new List<DefBase>();

        /// Backwards-compat filtered views. Older call sites in the editor
        /// (tree builder, save/verify flows, picker option lists) read recipes
        /// and "everything else" as two distinct collections; merging into a
        /// single Definitions list let the loader insert defs in file order
        /// without bookkeeping which sublist each one belonged to, but those
        /// readers still want the typed split. Both are lazy filters over
        /// the same backing list — mutations through these properties aren't
        /// supported; callers add through <see cref="Definitions"/> directly.
        public System.Collections.Generic.IEnumerable<RecipeDef> Recipes
            => System.Linq.Enumerable.OfType<RecipeDef>(Definitions);
        public System.Collections.Generic.IEnumerable<DefBase> OtherDefinitions
            => System.Linq.Enumerable.Where<DefBase>(Definitions, d => !(d is RecipeDef));

        /// Validation issues from the last load — currently
        /// use-before-definition warnings collected by
        /// <see cref="Io.PackValidator"/>. Surfaced to the modder via the
        /// editor's status banner / per-def warning rows so they can fix
        /// the file before it breaks the COI Python runtime on next load.
        public List<PackIssue> Issues = new List<PackIssue>();

        public PackModel(string modId, string rootPath) {
            ModId = modId;
            RootPath = rootPath;
        }
    }
}
