using System.Collections.Generic;
using Mafi;

namespace CustomAssets.Data.Mod {

    /// <summary>
    /// Versioned spec of every Python-side callable the framework exposes
    /// to packs. Modders pin <c>CustomAssets&gt;=X.Y.Z</c> in their
    /// manifest; this registry lets the runtime cross-check that the args
    /// they actually use are all available at (or before) that pinned
    /// version. Mismatches surface as warnings at registration time, with
    /// the pack name + file + line so the modder can either bump the pin
    /// or drop the unsupported argument.
    ///
    /// The registry is also injected into every pack's Python evaluation
    /// context as <c>API_Version</c> so packs can self-introspect:
    /// <code>
    ///     if "winding" in API_Version["add_unit_prefab"]["args"]:
    ///         add_unit_prefab(..., winding="cw")
    /// </code>
    ///
    /// Source-of-truth lives in C# (this file) — the Constructor
    /// declarations in <see cref="CustomAssetRegistrator"/> stay the
    /// canonical signature. A startup drift check walks both and warns
    /// when the latest registered arg list for a call diverges from the
    /// Constructor's actual <c>Arguments</c> array, so adding a new arg
    /// to a Constructor without recording it here is loud.
    ///
    /// Baseline rule: every call/arg listed below is assumed to have
    /// existed since <see cref="Baseline"/> (currently 0.1.0) unless the
    /// entry explicitly overrides <c>since</c>. Late additions get an
    /// explicit version so packs pinning an early CustomAssets see real
    /// warnings about real incompatibilities — nothing else.
    /// </summary>
    public static class ApiVersionRegistry {

        /// <summary>The CustomAssets framework's mod id. Captured by
        /// <see cref="CustomAssetsMod.RegisterPrototypes"/> at game-start
        /// so the pin-resolver can find the right entry in each pack's
        /// MandatoryDependencies array. Empty until that hook fires.</summary>
        public static string FrameworkModId = "";

        /// <summary>The CustomAssets framework's currently-running
        /// version, parsed from its own manifest. Captured by
        /// <see cref="CustomAssetsMod.RegisterPrototypes"/>. Default
        /// VersionSlim until that hook fires.</summary>
        public static VersionSlim FrameworkVersion;

        /// <summary>The earliest version of the framework with a known
        /// public API. Calls/args added before this point are treated as
        /// "always available". Bump only when an old call is removed.</summary>
        public static readonly VersionSlim Baseline = new VersionSlim(0, 1, 0);

        /// <summary>The full registry keyed by Python function name. Use
        /// <see cref="IsArgAvailableAt"/> / <see cref="IsCallAvailableAt"/>
        /// to query; reach in directly only for tooling that needs the
        /// raw spec.</summary>
        public static readonly Dictionary<string, ApiCallSpec> Calls = buildCalls();

        // ---- Query helpers --------------------------------------------------

        public static bool IsCallAvailableAt(string callName, VersionSlim pinnedVersion) {
            if (!Calls.TryGetValue(callName, out ApiCallSpec spec)) {
                // Unknown call — registry has no opinion. Don't pretend.
                return true;
            }
            return spec.Since <= pinnedVersion;
        }

        public static bool IsArgAvailableAt(string callName, string argName, VersionSlim pinnedVersion) {
            if (!Calls.TryGetValue(callName, out ApiCallSpec spec)) return true;
            VersionSlim argSince = spec.SinceForArg(argName);
            return argSince <= pinnedVersion;
        }

        public static VersionSlim SinceForCall(string callName) {
            return Calls.TryGetValue(callName, out ApiCallSpec spec) ? spec.Since : Baseline;
        }

        public static VersionSlim SinceForArg(string callName, string argName) {
            return Calls.TryGetValue(callName, out ApiCallSpec spec)
                ? spec.SinceForArg(argName)
                : Baseline;
        }

        // ---- Python projection ----------------------------------------------

        /// <summary>Build a Python-friendly Dictionary<string, object>
        /// snapshot of the entire registry. Shape:
        /// <code>
        /// API_Version["add_unit_prefab"] = {
        ///     "type": "call",
        ///     "since": "0.1.0",
        ///     "args": {
        ///         "winding": {"since": "0.2.0", "types": ["string"], "required": false, "default": "ccw"},
        ///         ...
        ///     }
        /// }
        /// </code>
        /// Args without an explicit registry entry are not present in
        /// the dict — modders should treat absence as "since the call
        /// itself was introduced" (i.e., the parent's <c>since</c>).</summary>
        public static Dictionary<string, object> ToPythonDict() {
            Dictionary<string, object> result = new Dictionary<string, object>(Calls.Count);
            foreach (KeyValuePair<string, ApiCallSpec> kv in Calls) {
                result[kv.Key] = kv.Value.ToPythonDict();
            }
            return result;
        }

        // ---- Registry build -------------------------------------------------

        // The build itself. Centralized so adding a new call is one
        // ".Call(name)" line and an arg-version override is a chained
        // ".Arg(name, version)" / ".RequiredArg(...)" call. Keep entries
        // alphabetical by category — easier to spot duplicates.
        private static Dictionary<string, ApiCallSpec> buildCalls() {
            ApiRegistryBuilder b = new ApiRegistryBuilder();

            // ---------- Products ------------------------------------
            b.Call("build_product_loose");
            b.Call("build_product_unit");
            b.Call("build_product_fluid");
            b.Call("product_exist");

            // ---------- Materials / textures / prefabs --------------
            b.Call("add_texture");
            b.Call("add_texture_material");
            b.Call("add_loose_product_material");
            b.Call("add_prefab_box");
            b.Call("add_unit_prefab")
                .Arg("mesh",    new VersionSlim(0, 1, 5))
                .Arg("winding", new VersionSlim(0, 2, 0), types: new[] { "string" }, defaultLiteral: "ccw");

            // ---------- Recipes / unlocks ---------------------------
            b.Call("build_recipe");
            b.Call("edit_recipe");
            b.Call("add_unlock_recipe");
            b.Call("add_unlock_product");
            b.Call("add_unlock_machine");

            // ---------- Research / edicts / categories --------------
            b.Call("build_research");
            b.Call("build_edict");
            b.Call("add_toolbar_category");

            // ---------- Machines / generators -----------------------
            b.Call("add_machine");
            b.Call("build_machine");
            b.Call("build_generator");
            b.Call("edit_machine_ports");

            // ---------- Settlement entities -------------------------
            b.Call("build_housing");
            b.Call("build_settlement_decoration");
            b.Call("build_settlement_food");
            b.Call("build_settlement_isp");
            b.Call("build_hospital");

            // ---------- Mines / labs --------------------------------
            b.Call("build_mine_tower");
            b.Call("build_research_lab");

            // ---------- Nuclear reactors ----------------------------
            // Both fuel-pair and the full Enrichment/fluid blocks
            // landed together as a 0.2.0 redesign — modders pinning
            // 0.1.x see the right warning if they use any of these.
            b.Call("build_nuclear_reactor")
                .Arg("fuel_pairs",          new VersionSlim(0, 2, 0))
                .Arg("fuelInPortShape",     new VersionSlim(0, 2, 0))
                .Arg("fuelOutPortShape",    new VersionSlim(0, 2, 0))
                .Arg("coolantIn",           new VersionSlim(0, 2, 0))
                .Arg("coolantOut",          new VersionSlim(0, 2, 0))
                .Arg("coolantInPort",       new VersionSlim(0, 2, 0))
                .Arg("coolantOutPort",      new VersionSlim(0, 2, 0))
                .Arg("coolantInPortShape",  new VersionSlim(0, 2, 0))
                .Arg("coolantOutPortShape", new VersionSlim(0, 2, 0))
                .Arg("waterInProduct",      new VersionSlim(0, 2, 0))
                .Arg("waterInQuantity",     new VersionSlim(0, 2, 0))
                .Arg("steamOutProduct",     new VersionSlim(0, 2, 0))
                .Arg("steamOutQuantity",    new VersionSlim(0, 2, 0))
                .Arg("waterInPorts",        new VersionSlim(0, 2, 0))
                .Arg("steamOutPorts",       new VersionSlim(0, 2, 0))
                .Arg("enrichment",          new VersionSlim(0, 2, 0))
                .Arg("add_ports",           new VersionSlim(0, 2, 0))
                .Arg("layout_str",          new VersionSlim(0, 2, 0));

            // Whole call shipped in 0.2.0 — packs pinning earlier get
            // a "this call requires >= 0.2.0" warning.
            b.Call("edit_nuclear_reactor_fuels",       new VersionSlim(0, 2, 0));
            b.Call("edit_nuclear_reactor_fluids",      new VersionSlim(0, 2, 0));
            b.Call("edit_nuclear_reactor_enrichment",  new VersionSlim(0, 2, 0));
            b.Call("edit_nuclear_reactor_ports",       new VersionSlim(0, 2, 0));

            // ---------- Data classes (used inside call args) --------
            b.Class("Percent");
            b.Class("Product");
            b.Class("Port");
            b.Class("FuelPair",      new VersionSlim(0, 2, 0));
            b.Class("Enrichment",    new VersionSlim(0, 2, 0));
            b.Class("EnrichmentStep",new VersionSlim(0, 2, 0));

            return b.Build();
        }
    }

    // ---- Data model ---------------------------------------------------------

    public sealed class ApiCallSpec {
        /// <summary>"call" for top-level callable; "class" for data
        /// constructors (Port / Product / FuelPair / ...).</summary>
        public string Kind = "call";
        public VersionSlim Since;
        /// <summary>Only contains entries for args with a NON-default
        /// SinceVersion. Args absent from the dictionary inherit the
        /// call's own <see cref="Since"/> — keeps the registry compact
        /// (most args don't need explicit version metadata).</summary>
        public Dictionary<string, ApiArgSpec> Args = new Dictionary<string, ApiArgSpec>();

        public VersionSlim SinceForArg(string argName) {
            return Args.TryGetValue(argName, out ApiArgSpec spec) ? spec.Since : Since;
        }

        public Dictionary<string, object> ToPythonDict() {
            Dictionary<string, object> d = new Dictionary<string, object> {
                ["type"]  = Kind,
                ["since"] = Since.ToString(),
            };
            if (Args.Count > 0) {
                Dictionary<string, object> args = new Dictionary<string, object>(Args.Count);
                foreach (KeyValuePair<string, ApiArgSpec> kv in Args) {
                    args[kv.Key] = kv.Value.ToPythonDict();
                }
                d["args"] = args;
            }
            return d;
        }
    }

    public sealed class ApiArgSpec {
        public VersionSlim Since;
        /// <summary>Optional — accepted Python/C# type names. Empty means
        /// "not documented"; the validator doesn't enforce types today,
        /// they're surfaced for IDE/documentation use.</summary>
        public List<string> Types = new List<string>();
        public bool Required;
        /// <summary>Optional default literal (e.g. "ccw", "False"). For
        /// docs only.</summary>
        public string DefaultLiteral = "";

        public Dictionary<string, object> ToPythonDict() {
            Dictionary<string, object> d = new Dictionary<string, object> {
                ["since"]    = Since.ToString(),
                ["required"] = Required,
            };
            if (Types.Count > 0) d["types"] = new List<object>(Types);
            if (!string.IsNullOrEmpty(DefaultLiteral)) d["default"] = DefaultLiteral;
            return d;
        }
    }

    // ---- Fluent builder -----------------------------------------------------

    /// <summary>
    /// Internal fluent builder for the static registry. Keeps the
    /// declaration site terse: one <c>.Call(name)</c> per public callable,
    /// optional <c>.Arg(name, since)</c> chains for late-added arguments.
    /// </summary>
    internal sealed class ApiRegistryBuilder {
        private readonly Dictionary<string, ApiCallSpec> m_calls = new Dictionary<string, ApiCallSpec>();
        private ApiCallSpec m_current;

        public ApiRegistryBuilder Call(string name) {
            return Call(name, ApiVersionRegistry.Baseline);
        }
        public ApiRegistryBuilder Call(string name, VersionSlim since) {
            m_current = new ApiCallSpec { Kind = "call", Since = since };
            m_calls[name] = m_current;
            return this;
        }
        public ApiRegistryBuilder Class(string name) {
            return Class(name, ApiVersionRegistry.Baseline);
        }
        public ApiRegistryBuilder Class(string name, VersionSlim since) {
            m_current = new ApiCallSpec { Kind = "class", Since = since };
            m_calls[name] = m_current;
            return this;
        }
        public ApiRegistryBuilder Arg(string name, VersionSlim since,
                string[] types = null, bool required = false, string defaultLiteral = "") {
            if (m_current == null) {
                Log.Warning("ApiRegistryBuilder.Arg('" + name + "') called before any Call/Class — ignored.");
                return this;
            }
            ApiArgSpec spec = new ApiArgSpec {
                Since = since,
                Required = required,
                DefaultLiteral = defaultLiteral ?? "",
            };
            if (types != null) spec.Types.AddRange(types);
            m_current.Args[name] = spec;
            return this;
        }
        public Dictionary<string, ApiCallSpec> Build() => m_calls;
    }
}
