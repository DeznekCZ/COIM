using CustomAssets.ModuleParser.Registrator;
using PythonAPI;
using CustomAssets.Utils;
using CustomAssets.Data.Translate;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Buildings.ResearchLab;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Population.Edicts;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.UnlockingTree;
using Mafi.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using PythonAPI.Statements;
using UnityEngine;
using static Mafi.Core.Prototypes.EntityCostsTpl;
using CustomAssets.Python;
using Mafi.Core.Entities;
using PythonAPI.Expressions;
using PythonAPI.Runtime;

namespace CustomAssets.Data.Mod;

public class CustomAssetRegistrator : IModData {

	private string m_modBasePath = ""; // SET IN RUNTIME, because of mod loading order
	private string m_modId = "";       // SET IN RUNTIME alongside m_modBasePath

	// Mod currently being loaded — mirrors m_modId into static context so the
	// static layout helpers (paramsWithCustomTokens, the define_box_type
	// resolver, makeBuildMachineCtor) can find the active pack's custom tile
	// tokens. Pack loads are sequential so a single static value is safe.
	private static string s_currentModId = "";

	// Per-pack custom layout tokens registered via define_box_type / layout_token.
	// Keyed by mod id; fed to COI's layout parser through paramsWithCustomTokens
	// so authored layout_str grids can use modder-defined tiles.
	private static readonly Dictionary<string, List<CustomLayoutToken>> s_customTokens =
		new Dictionary<string, List<CustomLayoutToken>>();

	// Rebuild an EntityLayoutParams from a source layout's params, ADDING this
	// pack's custom tokens. EntityLayoutParams is fully readonly, so every
	// field must be copied across — this single chokepoint keeps that copy in
	// one place if COI adds new params. When the pack registered no custom
	// tokens the source params are returned unchanged.
	private static EntityLayoutParams paramsWithCustomTokens(EntityLayout source) {
		EntityLayoutParams src = source.LayoutParams;
		if (!s_customTokens.TryGetValue(s_currentModId, out List<CustomLayoutToken> tokens)
				|| tokens.Count == 0) {
			return src;
		}
		List<CustomLayoutToken> merged = new List<CustomLayoutToken>();
		foreach (CustomLayoutToken t in src.CustomTokens) merged.Add(t);
		merged.AddRange(tokens);
		return new EntityLayoutParams(
			ignoreTilesForCore:           src.IgnoreTilesForCore.ValueOrNull,
			customTokens:                 merged,
			portsCanOnlyConnectToTransports: src.PortsCanOnlyConnectToTransports,
			hardenedFloorSurfaceId:       src.HardenedFloorSurfaceId,
			customVertexDataLayout:       src.CustomVertexDataLayout.ValueOrNull,
			customVertexTransformFn:      src.CustomVertexTransformFn.ValueOrNull,
			customMaxTerrainHeightFn:     src.CustomMaxTerrainHeightFn.ValueOrNull,
			customCollapseVerticesThreshold: src.CustomCollapseVerticesThreshold,
			customPlacementRange:         src.CustomPlacementRange,
			customPortHeights:            src.CustomPortHeights.IsEmpty
				? default(Option<IEnumerable<KeyValuePair<char, int>>>)
				: Option<IEnumerable<KeyValuePair<char, int>>>.Some(
					new List<KeyValuePair<char, int>>(src.CustomPortHeights.AsEnumerable())),
			enforceEmptySurface:          src.EnforceEmptySurface,
			minHeightAboveOcean:          src.MinHeightAboveOcean,
			tokenPostProcesssor:          src.TokenPostProcesssor.ValueOrNull);
	}

	private Dictionary<string, object> m_configValues = new Dictionary<string, object>();

	// Resolve an icon path. Two distinct cases:
	//   1. A raw string path like `Assets/Base/Products/Icons/Iron.svg` is a REFERENCE to an
	//      asset that lives in the base game's asset bundle (or another mod's). The framework
	//      does NOT attempt to load it from disk — the game's icon pipeline resolves it from
	//      the bundle at render time. Just pass it through.
	//   2. A path produced by add_texture(...) points at a file inside this mod's folder. For
	//      `.svg` we look for the build-time-generated companion `.png` (svg2png step in
	//      ModBuilder) and rewrite to that, since PNG loads faster than vector at runtime.
	//      If the PNG is missing we warn — the modder probably forgot to rebuild.
	//
	// The disambiguator is whether the file (svg or png) actually exists under m_modBasePath.
	// If neither does, we treat the path as a reference and stay quiet.
	private string ResolveIconPath(string path) {
		if (string.IsNullOrEmpty(path)) return path;
		if (!path.EndsWith(".svg", System.StringComparison.OrdinalIgnoreCase)) return path;

		string svgOnDisk = Path.Combine(m_modBasePath, path);
		if (!File.Exists(svgOnDisk)) {
			// Not a mod-local asset — it's a reference (vanilla bundle or external mod).
			// Use add_texture(...) when you want a real load from the mod folder.
			return path;
		}

		// Mod-local SVG: rewrite to its companion PNG produced by svg2png at build time.
		string pngPath = path.Substring(0, path.Length - 4) + ".png";
		string pngOnDisk = Path.Combine(m_modBasePath, pngPath);
		if (!File.Exists(pngOnDisk)) {
			Log.Warning($"ResolveIconPath: SVG '{path}' has no companion PNG at '{pngOnDisk}'. " +
				"Build the mod with ModBuilder (which runs svg2png) or commit the PNG manually.");
			return path;
		}
		return pngPath;
	}

	// Load a Texture2D from a mod-relative path, caching results in CustomAssetManager.Alternations.
	// mipChain:true is required because pile albedos get copied into a Texture2DArray slice that
	// has a full mip chain (without it the pile flickers as the camera moves).
	private Texture2D LoadModTexture(string texPath) {
		if (string.IsNullOrEmpty(texPath)) return null;
		if (CustomAssetManager.Alternations.TryGetValue(texPath, out UnityEngine.Object cached)) {
			if (cached is Texture2D cachedTex) return cachedTex;
			throw new ArgumentException($"Cached asset '{texPath}' is not a Texture2D.");
		}
		string filePath = Path.Combine(m_modBasePath, texPath);
		if (!File.Exists(filePath)) {
			Log.Warning($"LoadModTexture: file not found '{filePath}'");
			return null;
		}
		Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: true);
		if (!tex.LoadImage(File.ReadAllBytes(filePath))) {
			Log.Warning($"LoadModTexture: could not decode '{texPath}'");
			return null;
		}
		tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		CustomAssetManager.Alternations[texPath] = tex;
		tex.name = texPath;
		return tex;
	}

	// Cached reflection handle for ProductType.m_protoType (private field carrying the
	// System.Type that tells us whether a product is Fluid / Loose / Countable / Molten).
	private static readonly FieldInfo s_productTypeProtoTypeField = typeof(Mafi.Core.Products.ProductType)
		.GetField("m_protoType", BindingFlags.NonPublic | BindingFlags.Instance);

	// PortSpec is a struct with readonly fields, so it can't be constructed via object-
	// initializer syntax. We build one by boxing default(PortSpec) and writing each
	// field through reflection (which bypasses the readonly check). The boxed struct
	// is unboxed back to PortSpec on return.
	private static Mafi.Core.Ports.Io.PortSpec makePortSpec(
		char name,
		Mafi.IoPortType type,
		Mafi.Core.Ports.Io.IoPortShapeProto shape,
		bool canOnlyConnectToTransports)
	{
		object boxed = default(Mafi.Core.Ports.Io.PortSpec);
		typeof(Mafi.Core.Ports.Io.PortSpec).GetField("Name").SetValue(boxed, name);
		typeof(Mafi.Core.Ports.Io.PortSpec).GetField("Type").SetValue(boxed, type);
		typeof(Mafi.Core.Ports.Io.PortSpec).GetField("Shape").SetValue(boxed, shape);
		typeof(Mafi.Core.Ports.Io.PortSpec).GetField("CanOnlyConnectToTransports")
			.SetValue(boxed, canOnlyConnectToTransports);
		return (Mafi.Core.Ports.Io.PortSpec)boxed;
	}

	// Parse "input"/"output" (case-insensitive) into IoPortType. Throws on anything else
	// so the user gets a clear error at port-spec construction time rather than a cryptic
	// proto-ctor failure later.
	private static Mafi.IoPortType parsePortType(string s) {
		if (string.Equals(s, "input", StringComparison.OrdinalIgnoreCase)) return Mafi.IoPortType.Input;
		if (string.Equals(s, "output", StringComparison.OrdinalIgnoreCase)) return Mafi.IoPortType.Output;
		throw new ArgumentException($"Port type must be 'input' or 'output', got '{s}'.");
	}

	// Parse "+X", "-X", "+Y", "-Y" (and lowercase variants) into Direction90. Same rationale
	// as parsePortType — fail fast with a readable message.
	private static Mafi.Direction90 parseDirection(string s) {
		Mafi.Direction90? d = Mafi.Direction90.FromString(s);
		if (d.HasValue) return d.Value;
		throw new ArgumentException($"Port direction must be one of '+X','-X','+Y','-Y' (got '{s}').");
	}

	// Resolve an IoPortShapeProto by id. Used by edit_machine_ports / build_machine where
	// the modder passes an explicit shape string (e.g. "IoPortShape_Pipe").
	private static Mafi.Core.Ports.Io.IoPortShapeProto resolvePortShape(ProtosDb db, string shapeId) {
		return db.GetOrThrow<Mafi.Core.Ports.Io.IoPortShapeProto>(
			new Mafi.Core.Ports.Io.IoPortShapeProto.ID(shapeId));
	}

	// Resolve a Port(position=...) argument to a Vector3i. Accepts every
	// shape the Python lexer can produce for a positional literal:
	//   • Already-typed Vector3i (e.g. typed-ref like `Vector3i(6, 5, 0)`).
	//   • ValueTuple<int,int,int> / ValueTuple<int,int> (`(6, 5, 0)` / `(6, 5)`).
	//   • Any other ITuple of length 2-3 (long / float literals via Convert.ToInt32).
	//   • List<object> of 2-3 numeric items (for modders who write
	//     `position = [6, 5, 0]` with square brackets).
	// Throws a clear ArgumentException naming the actual value type if
	// nothing matches — the AnyArgument chain's empty "received: " trailer
	// was opaque on null and didn't surface ITuple-shaped mismatches.
	private static Vector3i parsePortPositionArg(object value) {
		if (value == null) {
			throw new ArgumentException(
				"Port: required argument `position` is missing or None. " +
				"Pass a tuple like (x, y, z) or (x, y), or a Vector3i.");
		}
		switch (value) {
			case Vector3i v3: return v3;
			case (int x, int y, int z): return new Vector3i(x, y, z);
			case (int x, int y): return new Vector3i(x, y, 0);
		}
		// ITuple covers ValueTuple<long,long,long>, Tuple<int,int>, etc.
		if (value is System.Runtime.CompilerServices.ITuple tup) {
			if (tup.Length == 3) {
				return new Vector3i(toIntOrThrow(tup[0]), toIntOrThrow(tup[1]), toIntOrThrow(tup[2]));
			}
			if (tup.Length == 2) {
				return new Vector3i(toIntOrThrow(tup[0]), toIntOrThrow(tup[1]), 0);
			}
		}
		if (value is List<object> list) {
			if (list.Count == 3) {
				return new Vector3i(toIntOrThrow(list[0]), toIntOrThrow(list[1]), toIntOrThrow(list[2]));
			}
			if (list.Count == 2) {
				return new Vector3i(toIntOrThrow(list[0]), toIntOrThrow(list[1]), 0);
			}
		}
		throw new ArgumentException(
			"Port: cannot interpret `position` of type " + value.GetType().FullName +
			". Expected (x, y, z) or (x, y) tuple of ints, or Vector3i.");
	}

	// Convert a boxed numeric (int / long / short / float / double) to int.
	// Used by parsePortPositionArg when the tuple element types vary —
	// Python literals always tokenize as int but typed-ref expressions
	// might return long or float values.
	private static int toIntOrThrow(object o) {
		if (o == null) throw new ArgumentException("Port.position: null component.");
		switch (o) {
			case int i:    return i;
			case long l:   return checked((int)l);
			case short s:  return s;
			case byte b:   return b;
			case float f:  return (int)f;
			case double d: return (int)d;
		}
		throw new ArgumentException(
			"Port.position: component of type " + o.GetType().FullName + " can't be converted to int.");
	}

	// Build an IoPortTemplate from one Python-side Port spec. Shape resolves through the
	// live PrototypesDb, direction/type via the parse helpers above.
	private static Mafi.Core.Ports.Io.IoPortTemplate buildIoPortTemplate(ProtosDb db, Port spec) {
		if (spec.name == default(char)) {
			throw new ArgumentException("Port.name is required (a single character port label).");
		}
		if (string.IsNullOrEmpty(spec.shape)) {
			throw new ArgumentException($"Port '{spec.name}' is missing required 'shape'.");
		}
		Mafi.IoPortType type = parsePortType(spec.type);
		Mafi.Direction90 dir = parseDirection(spec.direction);
		Mafi.Core.Ports.Io.IoPortShapeProto shape = resolvePortShape(db, spec.shape);
		Mafi.Core.Ports.Io.PortSpec ps = makePortSpec(spec.name, type, shape, spec.canOnlyConnectToTransports);
		return new Mafi.Core.Ports.Io.IoPortTemplate(
			ps,
			new Mafi.RelTile3i(spec.position.X, spec.position.Y, spec.position.Z),
			dir);
	}

	// Clone an EntityLayout, appending extraPorts to its Ports array. Preserves the source
	// tile/vertex data and forces the original CoreMin/CoreMax/LayoutSize via sizeOverride so
	// adding a port can never shift the building's footprint or its placement validation —
	// only its connectivity.
	private static EntityLayout layoutWithExtraPorts(
		EntityLayout src,
		IEnumerable<Mafi.Core.Ports.Io.IoPortTemplate> extraPorts)
	{
		var combined = src.Ports.AsEnumerable().Concat(extraPorts).ToImmutableArray();
		return new EntityLayout(
			src.SourceLayoutStr,
			src.LayoutTiles,
			src.TerrainVertices,
			combined,
			src.LayoutParams,
			src.CollapseVerticesThreshold,
			src.OriginTile,
			(src.CoreMin, src.CoreMax, src.LayoutSize));
	}

	// Per-port-name shape substitution. Walks the source layout's
	// IoPortTemplates and rebuilds each one with a new Shape proto if its
	// Spec.Name appears in <paramref name="portNameToNewShape"/>. Other
	// ports pass through verbatim. Returns a new EntityLayout with the
	// modified Ports array and all other fields shared from the source.
	//
	// Used by build_nuclear_reactor to swap the fuel-in / fuel-out port
	// shapes without touching the layout STRING — which avoids the
	// "two ports share the same shape char" ambiguity that the global
	// string.Replace approach can't resolve. The layout's SourceLayoutStr
	// is preserved (still round-trips through save) but the in-memory
	// Ports array carries the corrected shapes.
	private static EntityLayout layoutWithSubstitutedPortShapes(
		EntityLayout src,
		Dictionary<char, Mafi.Core.Ports.Io.IoPortShapeProto> portNameToNewShape)
	{
		if (portNameToNewShape == null || portNameToNewShape.Count == 0) return src;
		var rebuilt = new Lyst<Mafi.Core.Ports.Io.IoPortTemplate>();
		foreach (var p in src.Ports) {
			Mafi.Core.Ports.Io.IoPortShapeProto newShape;
			if (!portNameToNewShape.TryGetValue(p.Spec.Name, out newShape)) {
				// Not a fuel port — keep the existing template unchanged.
				rebuilt.Add(p);
				continue;
			}
			if (ReferenceEquals(newShape, p.Spec.Shape)) {
				// Caller asked for the same shape that's already there
				// (or the proto requested doesn't exist) — no-op.
				rebuilt.Add(p);
				continue;
			}
			var newSpec = makePortSpec(p.Spec.Name, p.Spec.Type, newShape,
				p.Spec.CanOnlyConnectToTransports);
			rebuilt.Add(new Mafi.Core.Ports.Io.IoPortTemplate(
				newSpec, p.RelativePosition, p.RelativeDirection));
		}
		return new EntityLayout(
			src.SourceLayoutStr,
			src.LayoutTiles,
			src.TerrainVertices,
			rebuilt.ToImmutableArray(),
			src.LayoutParams,
			src.CollapseVerticesThreshold,
			src.OriginTile,
			(src.CoreMin, src.CoreMax, src.LayoutSize));
	}

	// Same as layoutWithExtraPorts but REPLACES the source ports rather
	// than merging. Used by build_machine when `copy_ports=false`: the
	// modder wants the source layout's tile shape + terrain footprint
	// but a completely different set of port connections (typically
	// declared via `add_ports`).
	private static EntityLayout layoutWithReplacedPorts(
		EntityLayout src,
		IEnumerable<Mafi.Core.Ports.Io.IoPortTemplate> ports)
	{
		return new EntityLayout(
			src.SourceLayoutStr,
			src.LayoutTiles,
			src.TerrainVertices,
			(ports ?? Enumerable.Empty<Mafi.Core.Ports.Io.IoPortTemplate>()).ToImmutableArray(),
			src.LayoutParams,
			src.CollapseVerticesThreshold,
			src.OriginTile,
			(src.CoreMin, src.CoreMax, src.LayoutSize));
	}

	// Clone a source machine's Gfx into a fresh MachineProto.Gfx so the
	// new machine doesn't share the source's instance.
	//
	// Why the clone matters: LayoutEntityProto.Gfx.Initialize(proto) sets
	// `IconPath` based on the OWNING proto's id when IconIsCustom is false,
	// and it also writes a back-pointer `m_proto = proto`. Sharing the
	// source's Gfx between two protos means Initialize for the second
	// proto overwrites both fields, clobbering the source's icon and
	// breaking framework lookups that route through `m_proto`. The icon
	// loss the modder reported was that overwrite landing on a
	// not-yet-rendered "<sourceId>/<newId>.png" path that AssetsDb has
	// nothing registered at.
	//
	// We sidestep both issues by:
	//   1. Building a NEW Gfx instance (so source.Graphics and clone.Graphics
	//      are separate objects).
	//   2. Passing the source's resolved IconPath as `customIconPath`. That
	//      flips IconIsCustom to true, so Initialize won't reassign IconPath
	//      and the clone reuses the source's already-baked icon asset.
	//
	// The two animation-related private fields on the base
	// (instancedRenderingAnimationProtoSwap / ...MaterialSwap) aren't
	// public-readable; we pass null so animations behave the same as a
	// fresh Gfx would. In practice mod-side cloned machines that use
	// instanced rendering with animation swaps are rare, and the modder
	// can author a custom prefab if they need that combination.
	private static MachineProto.Gfx cloneMachineGfx(MachineProto source)
	{
		MachineProto.Gfx src = source.Graphics;
		return new MachineProto.Gfx(
			prefabPath:                          src.PrefabPath,
			categories:                          src.Categories,
			prefabOffset:                        src.PrefabOrigin,
			customIconPath:                      string.IsNullOrEmpty(src.IconPath)
													? default(Option<string>)
													: src.IconPath,
			particlesParams:                     src.ParticlesParams,
			emissionsParams:                     src.EmissionsParams,
			machineSoundPrefabPath:              src.MachineSoundPrefabPath,
			useInstancedRendering:               src.UseInstancedRendering,
			useSemiInstancedRendering:           src.UseSemiInstancedRendering,
			instancedRenderingExcludedObjects:   src.SemiInstancedRenderingExcludedObjects,
			hasSign:                             src.HasSign,
			color:                               src.Color,
			visualizedLayers:                    src.VisualizedLayers);
	}

	// Parse a Python `add_ports` argument (List<object> of Port instances) into a list of
	// constructed IoPortTemplates. Returns empty when the argument is absent.
	private List<Mafi.Core.Ports.Io.IoPortTemplate> parsePortList(
		ProtosDb db, Constructor.CallArguments args, string argName)
	{
		var result = new List<Mafi.Core.Ports.Io.IoPortTemplate>();
		if (!args.GetArgument<List<object>>(argName).WhenExists(out var raw)) return result;
		foreach (object item in raw) {
			if (item is Port p) {
				result.Add(buildIoPortTemplate(db, p));
			} else {
				throw new ArgumentException($"'{argName}' list item must be Port, got {item?.GetType()?.FullName ?? "null"}.");
			}
		}
		return result;
	}

	// Re-publish the (Layout, InputPorts, OutputPorts) trio on an existing LayoutEntityProto.
	// The base ctor populates InputPorts/OutputPorts from Layout.Ports — so after we replace
	// Layout we must rebuild those derived arrays too, or downstream code (recipe builder, IO
	// resolver, etc.) keeps using the old port set.
	private static void replaceLayoutOnProto(LayoutEntityProto proto, EntityLayout newLayout) {
		typeof(LayoutEntityProto)
			.GetProperty("Layout", BindingFlags.Public | BindingFlags.Instance)
			.GetSetMethod(nonPublic: true)
			?.Invoke(proto, new object[] { newLayout });
		// Property has no setter — fall back to backing field.
		FieldInfo layoutBacking = typeof(LayoutEntityProto)
			.GetField("<Layout>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
		layoutBacking?.SetValue(proto, newLayout);

		FieldInfo inputPortsField = typeof(LayoutEntityProto)
			.GetField("InputPorts", BindingFlags.Public | BindingFlags.Instance);
		FieldInfo outputPortsField = typeof(LayoutEntityProto)
			.GetField("OutputPorts", BindingFlags.Public | BindingFlags.Instance);
		inputPortsField?.SetValue(proto, newLayout.Ports
			.Where(pt => pt.Type == Mafi.IoPortType.Input).ToImmutableArray());
		outputPortsField?.SetValue(proto, newLayout.Ports
			.Where(pt => pt.Type == Mafi.IoPortType.Output).ToImmutableArray());
	}

	// Map a product to the IoPortShapeProto a conveyor/pipe needs in order to physically
	// connect to it. Used by build_generator to override port shapes after cloning the
	// source generator (since the source's ports are typed for ITS product, not ours).
	private static Mafi.Core.Ports.Io.IoPortShapeProto portShapeForProduct(
		ProtosDb db, Mafi.Core.Products.ProductProto product)
	{
		var protoType = s_productTypeProtoTypeField != null
			? (Type)s_productTypeProtoTypeField.GetValue(product.Type)
			: null;

		string shapeId;
		if (protoType == null) {
			throw new ArgumentException($"portShapeForProduct: cannot classify product '{product.Id.Value}' — m_protoType missing.");
		}
		if (typeof(Mafi.Core.Products.FluidProductProto).IsAssignableFrom(protoType))         shapeId = "IoPortShape_Pipe";
		else if (typeof(Mafi.Core.Products.MoltenProductProto).IsAssignableFrom(protoType))   shapeId = "IoPortShape_MoltenMetalChannel";
		else if (typeof(Mafi.Core.Products.LooseProductProto).IsAssignableFrom(protoType))    shapeId = "IoPortShape_LooseMaterialConveyor";
		else if (typeof(Mafi.Core.Products.CountableProductProto).IsAssignableFrom(protoType)) shapeId = "IoPortShape_FlatConveyor";
		else throw new ArgumentException(
			$"portShapeForProduct: unknown product type '{protoType.Name}' for '{product.Id.Value}'.");

		return db.GetOrThrow<Mafi.Core.Ports.Io.IoPortShapeProto>(
			new Mafi.Core.Ports.Io.IoPortShapeProto.ID(shapeId));
	}

	// Take the first character of `s` when non-empty, else fall back to
	// `defaultChar`. Used by build_nuclear_reactor / edit_nuclear_reactor_fluids
	// to resolve port-name char overrides where an empty modder value
	// means "inherit from source".
	private static char singleCharOrDefault(string s, char defaultChar) {
		if (string.IsNullOrEmpty(s)) return defaultChar;
		return s[0];
	}

	// Convert a Python-side Enrichment struct into a runtime
	// NuclearReactorProto.EnrichmentData. Each field falls back to the
	// SOURCE reactor's matching field when the modder left it at the
	// inherit-sentinel (null product, '\0' port char, -1 for ints,
	// destroyContentOnMeltdown == -1, null steps list). When the source
	// itself has no Enrichment, missing modder fields fall back to
	// defaults (default(char), PartialQuantity.Zero, etc.).
	//
	// Returns Option.None only when neither the modder NOR the source
	// supplied an Enrichment - in which case the cloned reactor doesn't
	// breed.
	private static Mafi.Option<NuclearReactorProto.EnrichmentData> buildEnrichmentDataFromPython(
		Enrichment py, Mafi.Option<NuclearReactorProto.EnrichmentData> sourceEnrichment)
	{
		NuclearReactorProto.EnrichmentData srcEd = sourceEnrichment.HasValue
			? sourceEnrichment.Value
			: null;

		Mafi.Core.Products.ProductProto inputProduct = py.inputProduct ?? srcEd?.InputProduct;
		Mafi.Core.Products.ProductProto outputProduct = py.outputProduct ?? srcEd?.OutputProduct;
		char inPort  = py.inPort  != default(char) ? py.inPort  : (srcEd?.InPort  ?? default(char));
		char outPort = py.outPort != default(char) ? py.outPort : (srcEd?.OutPort ?? default(char));

		Mafi.PartialQuantity processed;
		if (py.processedPerLevelNumerator >= 0) {
			processed = Mafi.PartialQuantity.FromFraction(
				py.processedPerLevelNumerator,
				py.processedPerLevelDenominator > 0 ? py.processedPerLevelDenominator : 1);
		} else if (srcEd != null) {
			processed = srcEd.ProcessedPerLevel;
		} else {
			processed = Mafi.PartialQuantity.Zero;
		}

		Mafi.Quantity buffers = py.buffersCapacity >= 0
			? new Mafi.Quantity(py.buffersCapacity)
			: (srcEd?.BuffersCapacity ?? new Mafi.Quantity(0));

		bool destroy = py.destroyContentOnMeltdown >= 0
			? py.destroyContentOnMeltdown == 1
			: (srcEd?.DestroyContentOnMeltdown ?? false);

		// Steps: when the modder supplied a list (even empty), use it;
		// otherwise inherit from source. Empty list isn't useful — the
		// reactor would have no breeding curve — but it's the modder's
		// choice and matches the explicit override semantics.
		Mafi.Collections.ImmutableCollections.ImmutableArray<NuclearReactorProto.EnrichmentStepData> steps;
		if (py.steps != null) {
			var lyst = new Mafi.Collections.Lyst<NuclearReactorProto.EnrichmentStepData>();
			foreach (EnrichmentStep s in py.steps) {
				lyst.Add(new NuclearReactorProto.EnrichmentStepData(
					Mafi.Percent.FromPercentVal(s.fuelMultiplierPercent),
					s.breedingRatio,
					s.steamReductionDiv));
			}
			steps = lyst.ToImmutableArray();
		} else if (srcEd != null) {
			steps = srcEd.EnrichmentSteps;
		} else {
			steps = Mafi.Collections.ImmutableCollections.ImmutableArray<NuclearReactorProto.EnrichmentStepData>.Empty;
		}

		int defaultStep = py.defaultEnrichmentStep >= 0
			? py.defaultEnrichmentStep
			: (srcEd?.DefaultEnrichmentStep ?? 0);

		// Source had no Enrichment AND the modder didn't supply enough
		// to construct one (no input/output product) - the cloned reactor
		// can't breed without those required fields. Pass through None.
		if (inputProduct == null || outputProduct == null) {
			return Mafi.Option<NuclearReactorProto.EnrichmentData>.None;
		}

		return new NuclearReactorProto.EnrichmentData(
			inputProduct, inPort, outputProduct, outPort,
			processed, buffers, destroy, steps, defaultStep);
	}

	// Resolve a single Tex/string OR a list of them into List<Texture2D>. Returns null when the
	// argument is absent and not required. Used by add_loose_product_material and add_unit_prefab.
	private List<Texture2D> ResolveTextureList(Constructor.CallArguments args, string argName, bool required) {
		if (args.GetArgument<List<object>>(argName).WhenExists(out var rawList)) {
			var result = new List<Texture2D>();
			foreach (var item in rawList) {
				switch (item) {
					case Tex t: result.Add(LoadModTexture(t.path)); break;
					case string s: result.Add(LoadModTexture(s)); break;
					case Texture2D t2d: result.Add(t2d); break;
					case null: result.Add(null); break;
					default: throw new ArgumentException(
						$"'{argName}' list item must be Tex or str, got {item.GetType()}");
				}
			}
			return result;
		}
		if (args.GetArgument<Tex>(argName)
			.When<string>(s => new Tex { path = s })
			.WhenExists(out Tex single)) {
			return new List<Texture2D> { LoadModTexture(single.path) };
		}
		if (required) {
			throw new ArgumentException($"'{argName}' is required.");
		}
		return null;
	}

	// Replicate single normals/metallic to match albedo count, or validate matching count.
	private static List<Texture2D> MatchTextureCount(List<Texture2D> list, int targetCount, string name) {
		if (list == null || list.Count == targetCount) return list;
		if (list.Count == 1) return Enumerable.Repeat(list[0], targetCount).ToList();
		throw new ArgumentException(
			$"'{name}' count ({list.Count}) must match albedo count ({targetCount}) or be a single texture.");
	}

	// Box mesh shared by add_prefab_box and add_unit_prefab's default geometry. Origin is at
	// the bottom-center (x in [-w/2, w/2], y in [0, h], z in [-d/2, d/2]) so a unit-product
	// prefab sits on the conveyor surface naturally.
	private static Mesh BuildBoxMesh(float width, float height, float depth) {
		float x = width * 0.5f, y = height, z = depth * 0.5f;
		var mesh = new Mesh();
		mesh.vertices = new[] {
			new Vector3(-x, 0, -z), new Vector3(x, 0, z), new Vector3(x, 0, -z), new Vector3(-x, 0, z),
			new Vector3(-x, y, -z), new Vector3(x, y, z), new Vector3(x, y, -z), new Vector3(-x, y, z),
		};
		mesh.triangles = new[] {
			0, 2, 1,  0, 3, 2,   // bottom
			4, 5, 6,  4, 6, 7,   // top
			0, 7, 3,  0, 4, 7,   // left
			1, 2, 6,  1, 6, 5,   // right
			3, 7, 6,  3, 6, 2,   // back
			0, 1, 5,  0, 5, 4,   // front
		};
		mesh.uv = new[] {
			new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
			new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
		};
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	public void RegisterData(ProtoRegistrator registrator) {
		m_modBasePath = registrator.ActiveMod.Manifest.RootDirectoryPath;
		m_modId = registrator.ActiveMod?.Manifest?.Id ?? "unknown-mod";
		s_currentModId = m_modId;
		// Reset this pack's custom-token list so a reload doesn't accumulate
		// duplicates from a previous load of the same mod.
		s_customTokens[m_modId] = new List<CustomLayoutToken>();
		m_configValues = ConfigLoader.Load(m_modBasePath);

		// Open the registry entry for this pack so register() can append parsed files as
		// they load. The editor reads this registry — it does not scan disk independently.
		PackRegistry.GetOrAdd(m_modId, m_modBasePath);

		// Stock primitive .obj files (wide_box, tall_box, flat_plate, cylinder_h,
		// cylinder_v, container_heap, custom_box) live under the core CustomAssets
		// mod's Assets/Primitives/ folder so every pack can reference them by the
		// same relative path. Generation is idempotent: existing files survive a
		// restart, only missing ones get written. Safe to call from every pack's
		// RegisterData — the static flag inside makes subsequent calls a no-op.
		PrimitiveStockGenerator.EnsureGenerated();

		// Splice this pack's Translations/<currentLang>.json into
		// LocalizationManager.s_data BEFORE any .py file runs. Each build_recipe /
		// build_research / build_product_* below causes Mafi to call Loc.Str(
		// "<protoId>__name", englishLiteral, ...) — which snapshots whatever
		// s_data currently has into the returned LocStr's frozen TranslatedString.
		// If the splice happens after the .py execution it has no effect; the
		// LocStr is born English and stays English. No-op in the default English
		// culture (s_data is null there) and when no translation file is shipped.
		ModTranslations.LoadForPack(m_modId, m_modBasePath);

		DiagnosticTrace.Step($"RegisterData: start (mod={m_modId}, base={m_modBasePath})");

		DirectoryInfo modules = new DirectoryInfo($"{m_modBasePath}/Definitions");
		Log.Info("Location of modules: " + modules.FullName);

		Set<string> loaded = [];
		Set<string> failed = [];

		StringBuilder failedLog = new StringBuilder();

		// If an __init__.py is present at the Definitions root, load it FIRST so it can
		// declare the explicit load order for the rest (via dependencies("...") calls or
		// any other initialization the pack wants). Files explicitly pulled in by
		// __init__.py end up in `loaded`, and the EnumerateFiles loop below skips them.
		// Legacy packs without __init__.py continue to work via auto-enumerate + each
		// file's own dependencies() declarations.
		// FAIL-FAST: if the first definition file fails to load (whether __init__.py or any
		// other), surface the error immediately as a CheckException instead of logging it
		// and pressing on with cascading failures. The log gets one clear root-cause entry
		// and the mod loader sees the exception at the right place.
		FileInfo initFile = new FileInfo(Path.Combine(modules.FullName, "__init__.py"));
		if (initFile.Exists) {
			DiagnosticTrace.Step($"RegisterData[{m_modId}]: __init__.py exists, loading");
			try {
				register(loaded, failed, registrator, initFile, modules);
				DiagnosticTrace.Step($"RegisterData[{m_modId}]: __init__.py loaded");
			} catch (Exception e) {
				DiagnosticTrace.Step($"RegisterData[{m_modId}]: __init__.py FAILED: {e.GetType().Name}: {e.Message}");
				Log.Error("Failed to load __init__.py");
				Log.Exception(e);
				failed.Add(initFile.FullName);
				throw new CheckException(
					$"Failed to load __init__.py: {e.Message}\n{e.StackTrace}", e);
			}
		}

		foreach (FileInfo enumerateFile in modules.EnumerateFiles("*.py")) {
			if (loaded.Contains(enumerateFile.FullName)) {
				continue;
			}
			if (failed.Contains(enumerateFile.FullName)) {
				continue;
			}
			DiagnosticTrace.Step($"RegisterData[{m_modId}]: enumerate-loading {enumerateFile.Name}");
			try {
				register(loaded, failed, registrator, enumerateFile, modules);
				DiagnosticTrace.Step($"RegisterData[{m_modId}]: enumerate-loaded {enumerateFile.Name}");
			} catch (Exception e) {
				string relPath = enumerateFile.FullName.Remove(0, modules.FullName.Length);
				DiagnosticTrace.Step($"RegisterData[{m_modId}]: {enumerateFile.Name} FAILED: {e.GetType().Name}: {e.Message}");
				Log.Error($"Failed to load definition: {relPath}");
				Log.Exception(e);
				failed.Add(enumerateFile.FullName);
				throw new CheckException(
					$"Failed to load definition '{relPath}': {e.Message}\n{e.StackTrace}", e);
			}
		}
		DiagnosticTrace.Step($"RegisterData[{m_modId}]: complete (loaded={loaded.Count}, failed={failed.Count})");

		// Per-pack API-version sanity check. Walks the AST we just
		// recorded, computes the minimum CustomAssets pin every call/arg
		// the pack uses requires, and compares against the pack's own
		// mod_dependencies pin. Out-of-date pins surface as a single
		// actionable warning with file/line context. Skip for the
		// framework's own mod — it has no pin to itself.
		if (!string.Equals(m_modId, ApiVersionRegistry.FrameworkModId, StringComparison.Ordinal)) {
			try {
				checkApiVersionForPack(registrator, m_modId);
			} catch (Exception ex) {
				Log.Warning($"[CustomAssets API] version check threw for '{m_modId}': {ex.Message}");
			}
		}

		// Note: we do NOT call CustomAssetManager.Instance.RunInjection() here. CAM may not yet
		// be constructed (it's lazy DI), and even if it were, it can't run before LPMM/
		// ProductsRenderer ctors anyway. Late injection for unit prefabs is handled by
		// CustomUnitPrefabHook, which depends on ProductsRenderer + CAM and patches the
		// renderer's cached CommonDataMutable for our products after both have constructed.
	}

	// Resolve the pack's pinned CustomAssets version from its
	// MandatoryDependencies array (the parsed mod_dependencies), then run
	// PackVersionAnalyzer over its files and compare. Logs the
	// recommended pin and a per-call breakdown when the modder is pinning
	// too low.
	private static void checkApiVersionForPack(ProtoRegistrator registrator, string modId) {
		if (!PackRegistry.TryGet(modId, out LoadedPack pack)) return;

		Mafi.VersionSlim pinned = resolveCustomAssetsPin(registrator);
		PackVersionAnalyzer.Report report = PackVersionAnalyzer.Analyze(pack);

		// Pack uses nothing newer than baseline → silent. No warning
		// even when the pin itself is missing; baseline works on any
		// framework version.
		if (report.RequiredVersion <= ApiVersionRegistry.Baseline) return;

		// Pin meets or exceeds what the pack actually uses → silent.
		// This is the happy path: well-pinned packs stay quiet.
		if (pinned.RawValue != 0 && pinned >= report.RequiredVersion) return;

		string pinnedDisplay = pinned.RawValue == 0
			? "(no CustomAssets pin in manifest.json)"
			: ">= " + pinned;
		Log.Warning(
			"[CustomAssets API] pack '" + modId + "' needs CustomAssets >= "
			+ report.RequiredVersion + " but manifest pins " + pinnedDisplay
			+ ". Bump 'mod_dependencies' to \"" + ApiVersionRegistry.FrameworkModId
			+ ">=" + report.RequiredVersion + "\" — or drop the late-version args below.");

		// Per-call detail — deduped by (call,arg) so a pack that uses
		// Enrichment in five recipes still prints one line per distinct
		// usage rather than five identical ones.
		HashSet<string> printed = new HashSet<string>();
		foreach (PackVersionAnalyzer.UsageNote note in report.Notes) {
			string key = note.Call + "|" + note.ArgName;
			if (!printed.Add(key)) continue;
			string what = string.IsNullOrEmpty(note.ArgName)
				? "  • call '" + note.Call + "' requires >= " + note.Required
				: "  • '" + note.ArgName + "' on " + note.Call + " requires >= " + note.Required;
			string where = note.Line > 0
				? " (" + note.FileName + ":" + note.Line + ")"
				: " (" + note.FileName + ")";
			Log.Warning(what + where);
		}
	}

	// Walk the ACTIVE pack's MandatoryDependencies and pull the
	// MinVersion of the entry whose Id matches the framework's captured
	// mod id. Returns default(VersionSlim) (raw=0) when no CustomAssets
	// pin is present — the caller treats that as "no constraint declared"
	// and the warning text says so explicitly.
	private static Mafi.VersionSlim resolveCustomAssetsPin(ProtoRegistrator registrator) {
		if (string.IsNullOrEmpty(ApiVersionRegistry.FrameworkModId)) {
			return default(Mafi.VersionSlim);
		}
		var deps = registrator?.ActiveMod?.Manifest.MandatoryDependencies;
		if (deps == null) return default(Mafi.VersionSlim);
		foreach (var dep in deps) {
			if (string.Equals(dep.Id, ApiVersionRegistry.FrameworkModId, StringComparison.Ordinal)) {
				return dep.MinVersion;
			}
		}
		return default(Mafi.VersionSlim);
	}

	private void register(Set<string> loaded, Set<string> failed, ProtoRegistrator registrator, FileInfo file,
		DirectoryInfo modules) {
		DiagnosticTrace.Step($"register: parsing {file.Name}");
		Token[] tokens = Tokenizer.ParseFile(file.FullName);
		Block block = Lexer.Parse(tokens);
		DiagnosticTrace.Step($"register: parsed {file.Name} ({tokens.Length} tokens, {block.statements.Count} statements)");

		// Record the parsed AST so the in-game editor can walk it later without
		// re-tokenising. Done before execution so even a file that throws during
		// Execute() leaves its parse tree visible to the editor.
		PackRegistry.RecordFile(m_modId, file.FullName, block);

		Dictionary<string, object> context = resolvers(registrator);
		context["dependencies"] = new Constructor([
			"dependencies"
		], (args => {
			foreach (object collection in args.Values()) {
				if (collection is string filename) {
					DiagnosticTrace.Step($"  dependencies: '{file.Name}' -> '{filename}'");
					FileInfo path = new FileInfo(Path.Combine(file.DirectoryName, filename + ".py"));

					if (false == path.Exists) {
						DiagnosticTrace.Step($"  dependencies: '{filename}' NOT FOUND at {path.FullName}");
						Log.Error($"Failed to load Custom Assets dependency: {file.Name} -> {filename}");
						throw new FileNotFoundException(
							$"Failed to load Custom Assets dependency: {file.Name} -> {filename}", path.FullName);
					}

					if (loaded.Contains(path.FullName)) {
						DiagnosticTrace.Step($"  dependencies: '{filename}' already loaded, skip");
						continue;
					}
					if (failed.Contains(path.FullName)) {
						// A previous dependency-load already failed and recorded this file.
						// Surface the situation as an exception so the OUTER file (the one
						// calling dependencies()) aborts too — otherwise we silently
						// continue with an incomplete prototype graph and the game hangs
						// later trying to resolve missing IDs.
						string relPath = path.FullName.Remove(0, modules.FullName.Length);
						Log.Error($"Invalid or missing dependency: {relPath}");
						throw new InvalidOperationException(
							$"Dependency '{relPath}' previously failed to load; cannot continue '{file.Name}'.");
					}
					try {
						register(loaded, failed, registrator, path, modules);
					} catch (Exception e) {
						Log.Error($"Failed to load dependency '{path.FullName.Remove(0, modules.FullName.Length)}' " +
							$"(referenced from '{file.Name}'): {e.Message}");
						Log.Exception(e);
						failed.Add(path.FullName);
						// Rethrow so the calling file's load aborts. The outer RegisterData
						// loop will record this file as failed too and report all failures
						// via the CheckException at the end.
						throw;
					}
				} else {
					throw new ArgumentException($"Type is not string: {collection.GetType()}", "dependencies");
				}
			}
			return null; // TODO dependencies as YIELD action in python context
		}));

		DiagnosticTrace.Step($"register: executing {block.statements.Count} statements in {file.Name}");
		int stmtIndex = 0;
		foreach (IStatement variable in block.statements) {
			DiagnosticTrace.Step($"register: {file.Name} statement[{stmtIndex}] {variable.GetType().Name}");
			try {
				variable.Execute(context);
			} catch (PythonParseException) {
				throw; // already carries file/line
			} catch (PythonRuntimeException) {
				throw; // already wrapped at a deeper frame
			} catch (Exception ex) {
				DiagnosticTrace.Step($"register: {file.Name} statement[{stmtIndex}] THREW {ex.GetType().Name}: {ex.Message}");
				(int startLine, int endLine) = getStatementLines(variable);
				throw new PythonRuntimeException(file, startLine, endLine, ex.Message, ex);
			}
			stmtIndex++;
		}

		loaded.Add(file.FullName);
		DiagnosticTrace.Step($"register: COMPLETED {file.Name}");
	}

	// Pull source-line range off the few statement types that track it
	// (AssignmentStatement, EvaluateStatement, IfStatement). Other top-level
	// statement kinds — class/function defs, imports — don't currently expose
	// a line, so we return (0,0) and the wrapper falls back to "file only".
	private static (int startLine, int endLine) getStatementLines(IStatement statement) {
		switch (statement) {
			case AssignmentStatement asn:
				return (asn.StartLine, asn.EndLine);
			case EvaluateStatement ev:
				return (ev.StartLine, ev.EndLine);
			case IfStatement ifs:
				return (ifs.StartLine, ifs.EndLine);
			default:
				return (0, 0);
		}
	}

	private static Texture2D NONE = new Texture2D(2, 2, TextureFormat.ARGB32, false);

	private Dictionary<string, object> resolvers(ProtoRegistrator registrator) {
		Dictionary<string, object> context = new Dictionary<string, object> {

			#region Mod config (config.json defaults — exposed as `config.<field>` to .py)

			["config"] = m_configValues,

			#endregion

			#region API version registry (exposed as `API_Version[name]` to .py)

			// Packs can self-introspect at runtime — e.g.
			//     if "winding" in API_Version["add_unit_prefab"]["args"]:
			//         add_unit_prefab(..., winding="cw")
			// Schema documented on ApiVersionRegistry.ToPythonDict.
			["API_Version"] = ApiVersionRegistry.ToPythonDict(),

			#endregion

			#region Types

			["RecipeProto"] = typeof(RecipeProto),
			["MachineProto"] = typeof(MachineProto),
			["ProductProto"] = typeof(ProductProto),
			["ResearchNodeProto"] = typeof(ResearchNodeProto),
			["Duration"] = typeof(Duration),
			["Quantity"] = typeof(Quantity),
			["Percent"] = new Constructor(["value"], args => Expressions.__int__(args[0].Value).Percent()),
			["Proto"] = typeof(Proto),
			["Vector3i"] = typeof(Vector3i),
			["Vector3f"] = typeof(Vector3f),
			["Vector2i"] = typeof(Vector2i),
			["Vector2f"] = typeof(Vector2f),
			["Prefab"] = typeof(Prefab), //TODO
			["Mat"] = typeof(Mat), //TODO
			["Tex"] = typeof(Tex), //TODO

			#endregion

			#region Texture

			["add_texture"] = new Constructor(["path", "replace"], (args) => {
				string assetPath = args.GetArgument<string>("path").ElseRequiredThrow();

				// Two acceptance shapes:
				//   â€¢ Mod-local: path resolves to a file under m_modBasePath
				//     â†’ load the bytes synchronously and cache in
				//     Alternations under the (possibly remapped) asset path.
				//   â€¢ Game asset: file does not exist in the mod folder
				//     â†’ return a marker Tex that downstream consumers
				//     (add_prefab_box / add_unit_prefab / add_texture_material)
				//     resolve against AssetsDb at injection time. Proto
				//     registration runs BEFORE the asset bundles are loaded,
				//     so we can't fetch a Texture2D here without crashing.
				string filePath = Path.Combine(m_modBasePath, assetPath);

				if (args.GetArgument<string>("replace").WhenExists(out string replacementAssetPath)) {
					assetPath = replacementAssetPath;
				}

				Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
				byte[] image = File.ReadAllBytes(filePath);
				if (!texture2D.LoadImage(image)) {
					throw new ArgumentException($"Could not load an image: {assetPath}");
				}

				texture2D.name = assetPath;
				CustomAssetManager.Alternations[assetPath] = texture2D;
				return new Tex { path = assetPath, loadedAsset = true };
			}),

			#endregion

			#region Texture_Material

			["add_texture_material"] = new Constructor(["path", "texture", "reference", "shader"], (args) => {
				// Texture is optional when a reference material is provided: missing texture
				// means "clone the reference unchanged" so modders can iterate visuals later.
				Texture2D texture2D = null;
				if (args.GetArgument<Tex>("texture")
					.When<string>(pathTex => new Tex { path = pathTex })
					.WhenExists(out Tex texture)) {
					if (CustomAssetManager.Alternations.TryGetValue(texture.path, out UnityEngine.Object data)) {
						if (data is Texture2D t2d) {
							texture2D = t2d;
						} else {
							throw new ArgumentException($"Given prefab is not a Texture: {texture.path}");
						}
					} else {
						string filePath = Path.Combine(m_modBasePath, texture.path);
						if (File.Exists(filePath)) {
							// mipChain: true so Apply() below generates mipmaps. This is required
							// when the texture is later copied into a Texture2DArray slice that has
							// mips (e.g. LooseProductMaterialManager.AlbedoTexArray). Without mip-
							// maps the pile flickers as the camera moves and Unity samples missing
							// mip levels.
							texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: true);
							byte[] image = File.ReadAllBytes(filePath);
							if (!texture2D.LoadImage(image)) {
								Log.Warning($"add_texture_material: could not decode image '{texture.path}' " +
									"in mod folder; material will use the reference's original albedo.");
								texture2D = null;
							} else {
								texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: false);
								CustomAssetManager.Alternations.Add(texture.path, texture2D);
								texture2D.name = texture.path;
							}
						} else {
							Log.Warning($"add_texture_material: texture file '{filePath}' not found; " +
								"material will use the reference's original albedo (drop in the file later).");
						}
					}
				}

				string path = args.GetArgument<string>("path").ElseRequiredThrow();

				// Optional reference material to clone (matches its shader + all properties),
				// and/or an explicit shader name. If either is set, defer construction so we can
				// resolve the reference through AssetsDb at injection time. Otherwise keep the
				// legacy Standard-shader path for back-compat.
				string referencePath = null;
				if (args.GetArgument<Mat>("reference")
					.When<string>(s => new Mat { path = s })
					.WhenExists(out Mat refMat)) {
					referencePath = refMat?.path;
				}
				string shaderName = args.GetArgument<string>("shader").ElseDefault(null);

				if (!string.IsNullOrEmpty(referencePath) || !string.IsNullOrEmpty(shaderName)) {
					CustomAssetManager.PendingMaterials.Add(new DeferredMaterial {
						OutputPath = path,
						NewTexture = texture2D,
						ReferencePath = referencePath,
						ShaderName = shaderName,
					});
				} else {
					if (texture2D == null) {
						throw new ArgumentException(
							"add_texture_material with no 'reference' or 'shader' requires a 'texture' argument; " +
							"the legacy Standard-shader path needs an albedo texture.");
					}
					Material material = new Material(Shader.Find("Standard"));
					material.CopyPropertiesFromMaterial(new AssetsDb().DefaultMaterial);
					material.mainTexture = texture2D;
					material.SetTexture(Shader.PropertyToID("_AlbedoTex"), texture2D);
					material.color = Color.white;
					CustomAssetManager.Alternations.Add(path, material);
				}
				return new Mat { path = path };
			}),

			#endregion

			#region Loose_Product_Material

			["add_loose_product_material"] = new Constructor(
				["path", "albedo", "normals", "metallic", "reference", "tiling"],
				(args) => {
					string path = args.GetArgument<string>("path").ElseRequiredThrow();

					List<Texture2D> albedos = ResolveTextureList(args, "albedo", required: true);
					List<Texture2D> normals = ResolveTextureList(args, "normals", required: false);
					List<Texture2D> metallics = ResolveTextureList(args, "metallic", required: false);
					normals = MatchTextureCount(normals, albedos.Count, "normals");
					metallics = MatchTextureCount(metallics, albedos.Count, "metallic");

					// Reference is optional. When omitted, fall back to a known vanilla pile material
					// so the cloned Material carries the right shader + property layout (Mafi's pile
					// shader is bundled, not Shader.Find-able). Normals/metallic that the user didn't
					// override are then "copied from reference" via the Material clone.
					string referencePath = null;
					if (args.GetArgument<Mat>("reference")
						.When<string>(s => new Mat { path = s })
						.WhenExists(out Mat refMat)) {
						referencePath = refMat?.path;
					}
					if (string.IsNullOrEmpty(referencePath)) {
						// NOTE: actual asset path uses ".mat" extension, NOT a "_mat" suffix.
						// The C# constant in Mafi.Base is *named* FilterMedia_mat but its value
						// is "Assets/Base/Products/Loose/FilterMedia.mat" — easy to confuse.
						referencePath = Mafi.Base.Assets.Base.Products.Loose.FilterMedia_mat;
					}

					float tiling = args.GetNumberArgument<float>("tiling")
						.ElseDefault(1f);

					CustomAssetManager.PendingMaterials.Add(new DeferredMaterial {
						OutputPath = path,
						AlbedoArray = albedos,
						NormalsArray = normals,
						SmoothMetalArray = metallics,
						ReferencePath = referencePath,
						Tiling = tiling,
					});
					return new Mat { path = path };
				}),

			#endregion

			#region Unit_Product_Prefab

			["add_unit_prefab"] = new Constructor(
				["path", "albedo", "normals", "metallic", "reference",
				 "width", "height", "depth", "mesh", "winding"],
				(args) => {
					string path = args.GetArgument<string>("path").ElseRequiredThrow();

					// Mesh winding override. Default "ccw" passes the source corners
					// through (correct for standard OBJ exports from Blender / Maya /
					// 3ds Max — Unity treats CCW as front-facing). Set to "cw" for
					// CW-authored OBJs that would otherwise render inside-out.
					bool reverseWinding = false;
					if (args.GetArgument<string>("winding").WhenExists(out string windingStr)) {
						string normalized = (windingStr ?? "").Trim().ToLowerInvariant();
						switch (normalized) {
							case "":
							case "ccw":
								reverseWinding = false;
								break;
							case "cw":
								reverseWinding = true;
								break;
							default:
								throw new ArgumentException(
									"add_unit_prefab: 'winding' must be \"ccw\" or \"cw\", got \"" + windingStr + "\".");
						}
					}

					// Reuse the loose-material plumbing for textures: a unit-product material
					// is conceptually identical (one albedo + optional normal/metallic clone).
					List<Texture2D> albedos = ResolveTextureList(args, "albedo", required: true);
					List<Texture2D> normals = ResolveTextureList(args, "normals", required: false);
					List<Texture2D> metallics = ResolveTextureList(args, "metallic", required: false);
					normals = MatchTextureCount(normals, albedos.Count, "normals");
					metallics = MatchTextureCount(metallics, albedos.Count, "metallic");

					string referencePath = null;
					if (args.GetArgument<Mat>("reference")
						.When<string>(s => new Mat { path = s })
						.WhenExists(out Mat refMat)) {
						referencePath = refMat?.path;
					}
					// Unit prefabs default to the Standard shader when no reference is given —
					// unlike pile materials, the unit-product render path doesn't need a Mafi-
					// specific shader. add_prefab_box has been doing this since day one.

					// Mesh: load .obj if `mesh` was given, otherwise generate a box from
					// width/height/depth. .obj wins if both are present. The .obj path doubles
					// as a cache key — multiple add_unit_prefab calls with the same `mesh` share
					// a single Mesh instance (matches Unity's bundle-asset sharing behaviour).
					Mesh mesh = null;
					if (args.GetArgument<string>("mesh").WhenExists(out string objRel)) {
						// Cache key includes winding so two prefabs that share an .obj path
						// but request different winding don't collide on the cache.
						string cacheKey = reverseWinding ? objRel + "::cw" : objRel;
						if (!CustomAssetManager.Meshes.TryGetValue(cacheKey, out mesh)) {
							// Resolve order: pack-local first, then fall back
							// to the stock-primitives folder under the core
							// CustomAssets mod. Lets a pack overlay a stock
							// primitive by dropping a same-named .obj into
							// its own Assets/Primitives/ folder, while every
							// pack can reference the seven shipped presets
							// with no per-pack copy.
							string objFull = Path.Combine(m_modBasePath, objRel);
							if (!File.Exists(objFull)) {
								string coreRoot = PrimitiveStockGenerator.ResolveCoreModRoot();
								if (!string.IsNullOrEmpty(coreRoot)) {
									string coreCandidate = Path.Combine(coreRoot, objRel);
									if (File.Exists(coreCandidate)) {
										objFull = coreCandidate;
									}
								}
							}
							mesh = ObjLoader.LoadFromFile(objFull, reverseWinding);
							if (mesh != null) {
								mesh.name = cacheKey;
								CustomAssetManager.Meshes[cacheKey] = mesh;
							}
						}
						if (mesh == null) {
							Log.Warning($"add_unit_prefab: failed to load mesh '{objRel}'; falling back to default box.");
						}
					}
					if (mesh == null) {
						float w = args.GetNumberArgument<float>("width").ElseDefault(0.5f);
						float h = args.GetNumberArgument<float>("height").ElseDefault(0.2f);
						float d = args.GetNumberArgument<float>("depth").ElseDefault(0.5f);
						mesh = BuildBoxMesh(w, h, d);
						mesh.name = path + "__mesh";
					}

					// Build the deferred material at <path>__material so it doesn't collide with
					// the prefab path. The prefab references this material by path; both end up
					// in LoadedAssets at injection time.
					string materialPath = path + "__material";
					CustomAssetManager.PendingMaterials.Add(new DeferredMaterial {
						OutputPath = materialPath,
						AlbedoArray = albedos,
						NormalsArray = normals,
						SmoothMetalArray = metallics,
						ReferencePath = referencePath,
					});

					CustomAssetManager.PendingPrefabs.Add(new DeferredPrefab {
						OutputPath = path,
						Mesh = mesh,
						MaterialPath = materialPath,
					});
					return new Prefab { path = path };
				}),

			#endregion

			#region Model

			["add_prefab_box"] = new Constructor(["path", "texture"], (args) => {
				// Captured for the deferred AssetsDb lookup that runs in
				// CustomAssetManager.EnsureInjected. Null when the texture
				// is already a concrete Texture2D (legacy direct form) or
				// a Tex marked mod-local.
				string pendingGameTexturePath = null;

				Texture2D texture = args.GetArgument<Texture2D>("texture")
					.When<Tex>(tex => {
						if (tex == null || string.IsNullOrEmpty(tex.path)) {
							throw new ArgumentException("add_prefab_box: 'texture' Tex has empty path");
						}
						if (!tex.loadedAsset) {
							// Defer: record the path; the prefab's material gets a
							// placeholder texture for now and the real Texture2D
							// lands at injection time once AssetsDb is populated.
							pendingGameTexturePath = tex.path;
							return NONE;
						}
						// Mod-local: add_texture(...) cached the Texture2D under
						// tex.path in Alternations. Pull it from there.
						if (CustomAssetManager.Alternations.TryGetValue(tex.path, out UnityEngine.Object data)
								&& data is Texture2D modTex) {
							return modTex;
						}
						throw new ArgumentException(
							$"add_prefab_box: 'texture' Tex '{tex.path}' not found in Alternations cache. "
							+ "Was add_texture(...) called for this path?");
					})
					.When<string>(pathTex => {
						if (CustomAssetManager.Alternations.TryGetValue(pathTex, out UnityEngine.Object data)) {
							return data is Texture2D t2d
								? t2d
								: throw new ArgumentException($"Given prefab is not a Texture: {pathTex}");
						}

						// Mod-local first: when the path resolves to a real
						// file under the mod folder, load + cache directly.
						// This is the common case for textures shipped
						// alongside the pack.
						string modLocalPath = Path.Combine(m_modBasePath, pathTex);
						if (File.Exists(modLocalPath)) {
							Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
							byte[] image = File.ReadAllBytes(modLocalPath);
							if (!texture2D.LoadImage(image)) {
								throw new ArgumentException($"Could not load an image: {pathTex}");
							}
							CustomAssetManager.Alternations.Add(pathTex, texture2D);
							return texture2D;
						}

						// Not on disk under the mod folder - treat as a
						// game-asset path (e.g. "Assets/Base/Products/.../X.png").
						// Same deferred-injection path the Tex branch uses
						// for !tex.loadedAsset: record the path; the prefab's
						// material gets the NONE placeholder for now and the
						// real Texture2D lands at injection time once
						// AssetsDb is populated.
						pendingGameTexturePath = pathTex;
						return NONE;
					})
					.ElseRequiredThrow();

				string path = args.GetArgument<string>("path").ElseRequiredThrow();
				GameObject prefab = new GameObject();
				prefab.SetActive(true);
				prefab.name = path;

				MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
				Mesh mesh = new Mesh();
				float x = args.GetArgument<float>("width").ElseDefault(0.5f);
				float y = args.GetArgument<float>("height").ElseDefault(0.2f);
				float z = args.GetArgument<float>("depth").ElseDefault(0.5f);
				mesh.vertices = new[] {
					new Vector3(-x, 0, -z),
					new Vector3(x, 0, z),
					new Vector3(x, 0, -z),
					new Vector3(-x, 0, z),
					new Vector3(-x, y, -z),
					new Vector3(x, y, z),
					new Vector3(x, y, -z),
					new Vector3(-x, y, z),
				};
				mesh.triangles = new[] {
					// Front face
					0, 2, 1,
					0, 3, 2,
					// Back face
					4, 5, 6,
					4, 6, 7,
					// Left face
					0, 7, 3,
					0, 4, 7,
					// Right face
					1, 2, 6,
					1, 6, 5,
					// Top face
					3, 7, 6,
					3, 6, 2,
					// Bottom face
					0, 1, 5,
					0, 5, 4
				};
				mesh.uv = new[] {
					new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
					new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
				};
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();
				meshFilter.mesh = mesh;

				MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
				meshRenderer.material = new Material(Shader.Find("Standard"));
				meshRenderer.material.mainTexture = texture;

				CustomAssetManager.Alternations.Add(path, prefab);
				CustomAssetManager.Alternations.Add(path + "__mesh", meshFilter.mesh);
				CustomAssetManager.Alternations.Add(path + "__material", meshRenderer.material);
				// If the texture came from add_texture(game-asset) the
				// material currently has a null mainTexture. Schedule the
				// resolution against AssetsDb in CustomAssetManager's
				// injection pass â€” it walks PendingGameTextures and pushes
				// the loaded Texture2D onto the prefab's material.
				if (pendingGameTexturePath != null) {
					CustomAssetManager.PendingGameTextures[path] = pendingGameTexturePath;
				}
				return new Prefab() { path = path };
			}),

			#endregion

			#region Build research

			["build_research"] = new Constructor(
				["researchId", "name", "description", "difficulty", "position", "parents"], (args) => {
					var builder = registrator.ResearchNodeProtoBuilder.Start(
						name: args.GetArgument<string>("name")
							.ElseRequiredThrow(),
						nodeId: args.GetArgument<ResearchNodeProto.ID>("researchId")
							.When<string>(s => new ResearchNodeProto.ID(s))
							.ElseRequiredThrow(),
						costMonths: args.GetArgument<int>("costs").ElseDefault(1));

					if (args.GetArgument<List<ResearchNodeProto>>("parents")
						.When<List<object>>(o => {
							List<ResearchNodeProto> protosCollector = new List<ResearchNodeProto>();
							foreach (object item in o) {
								switch (item) {
								case ResearchNodeProto proto: protosCollector.Add(proto); break;
								case ResearchNodeProto.ID id:
									protosCollector.Add(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id));
									break;
								case string sid:
									protosCollector.Add(
										registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
											new ResearchNodeProto.ID(sid)));
									break;
								default: throw new ProtoBuilderException($"Proto '{item}' was not found.");
								}
							}
							return protosCollector;
						}).WhenExists(out var protos)) {
						builder.AddParents(protos.ToArray());
					}

					if (args.GetArgument<Tex>("icon")
						.When<string>(s => new Tex { path = s })
						.WhenExists(out Tex path)) {
						builder.AddIcon(ResolveIconPath(path.path));
					}

					ResearchNodeProto node = builder.BuildAndAdd();
					Vector2i position = args.GetArgument<Vector2i>("position")
						.When<(int x, int y)>(pt => new Vector2i(pt.x, pt.y))
						.ElseDefault(Vector2i.Zero);
					node.GridPositionWherePossible(registrator.PrototypesDb, position);
					return node;
				}),

			#endregion

			#region Existence checkers

			// Returns true if a ProductProto with the given id is registered. Useful for
			// gating optional recipes on the presence of products from other mods or game
			// versions, e.g.:
			//     if product_exist("Product_FilterMediaIronLime"):
			//         build_recipe(...)
			["product_exist"] = new Constructor(["product"], (args) => {
				return (object)args.GetArgument<bool>("product")
					.When<ProductProto>(p => p != null)
					.When<ProductProto.ID>(pid =>
						registrator.PrototypesDb.TryGetProto<ProductProto>((Proto.ID)pid, out _))
					.When<string>(s =>
						registrator.PrototypesDb.TryGetProto<ProductProto>((Proto.ID)new ProductProto.ID(s), out _))
					.ElseRequiredThrow();
			}),

			#endregion

			#region Define product reference for recipe creation

			["Product"] = new Constructor(["product", "quantity", "port"], (args) => {
				return new Product() {
					product = args.GetArgument<ProductProto>("product")
						.When<ProductProto.ID>(id
							=> registrator.PrototypesDb.GetOrThrow<ProductProto>((Proto.ID)id))
						.When<string>(id
							=> registrator.PrototypesDb.GetOrThrow<ProductProto>((Proto.ID)new ProductProto.ID(id)))
						.ElseRequiredThrow(),
					quantity = args.GetArgument<Quantity>("quantity")
						.When<int>(v => new Quantity(v))
						.ElseRequiredThrow(),
					port = args.GetArgument<string>("port")
						.ElseDefault("*"),
				};
			}),

			#endregion

			#region Define fuel-pair reference for build_nuclear_reactor

			// Python-side `FuelPair(fuelIn, spentFuelOut, durationSeconds)`
			// → resolves products via ProtosDb at runtime. Stored in the
			// shared FuelPair struct (defined at the bottom of this file)
			// so build_nuclear_reactor's resolver can pick them up from
			// the add-list verbatim.
			["FuelPair"] = new Constructor(
				["fuelIn", "spentFuelOut", "durationSeconds"], args => {
					Mafi.Core.Products.ProductProto fuelIn = args.GetArgument<Mafi.Core.Products.ProductProto>("fuelIn")
						.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
						.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
						.ElseRequiredThrow();
					Mafi.Core.Products.ProductProto spentFuelOut = args.GetArgument<Mafi.Core.Products.ProductProto>("spentFuelOut")
						.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
						.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
						.ElseRequiredThrow();
					int durationSec = args.GetArgument<int>("durationSeconds").ElseRequiredThrow();
					return new FuelPair {
						fuelIn = fuelIn,
						spentFuelOut = spentFuelOut,
						durationSeconds = durationSec,
					};
				}),

			// Enrichment / EnrichmentStep are layered the same way as
			// FuelPair — Python factory → POD struct → the consumer
			// (build_nuclear_reactor / edit_nuclear_reactor_enrichment)
			// converts to runtime types at apply time. Every field on
			// Enrichment is optional; the Python caller passes only the
			// pieces they want to override and the resolver merges
			// against the source reactor's EnrichmentData at apply time.
			["EnrichmentStep"] = new Constructor(
				["fuelMultiplierPercent", "breedingRatio", "steamReductionDiv"], args => {
					return new EnrichmentStep {
						fuelMultiplierPercent = args.GetArgument<int>("fuelMultiplierPercent").ElseDefault(100),
						breedingRatio         = args.GetArgument<int>("breedingRatio").ElseDefault(0),
						steamReductionDiv     = args.GetArgument<int>("steamReductionDiv").ElseDefault(1),
					};
				}),

			["Enrichment"] = new Constructor(
				["inputProduct", "inPort", "outputProduct", "outPort",
				 "processedPerLevel", "buffersCapacity", "destroyContentOnMeltdown",
				 "defaultEnrichmentStep", "steps"],
				args => {
					Mafi.Core.Products.ProductProto inputProto = args.GetArgument<Mafi.Core.Products.ProductProto>("inputProduct")
						.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
						.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
						.ElseNull();
					Mafi.Core.Products.ProductProto outputProto = args.GetArgument<Mafi.Core.Products.ProductProto>("outputProduct")
						.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
						.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
						.ElseNull();
					string inPortStr  = args.GetArgument<string>("inPort").ElseDefault(null);
					string outPortStr = args.GetArgument<string>("outPort").ElseDefault(null);
					int processedNum = -1, processedDen = 1;
					object rawProcessed = args["processedPerLevel"]?.Value;
					if (rawProcessed is int pInt) {
						processedNum = pInt;
					} else if (rawProcessed is System.ValueTuple<int, int> tuple) {
						processedNum = tuple.Item1;
						processedDen = tuple.Item2;
					}
					int destroyTriState = -1;
					if (args.GetArgument<bool>("destroyContentOnMeltdown").WhenExists(out bool destroyVal)) {
						destroyTriState = destroyVal ? 1 : 0;
					}
					System.Collections.Generic.List<EnrichmentStep> steps = null;
					if (args.GetArgument<System.Collections.Generic.List<object>>("steps").WhenExists(out var rawSteps)) {
						steps = new System.Collections.Generic.List<EnrichmentStep>();
						foreach (object item in rawSteps) {
							if (item is EnrichmentStep es) steps.Add(es);
							else throw new ArgumentException(
								"Enrichment.steps list item must be EnrichmentStep, got " +
								(item?.GetType()?.FullName ?? "null") + ".");
						}
					}
					return new Enrichment {
						inputProduct                 = inputProto,
						inPort                       = (inPortStr?.Length > 0) ? inPortStr[0] : default(char),
						outputProduct                = outputProto,
						outPort                      = (outPortStr?.Length > 0) ? outPortStr[0] : default(char),
						processedPerLevelNumerator   = processedNum,
						processedPerLevelDenominator = processedDen,
						buffersCapacity              = args.GetArgument<int>("buffersCapacity").ElseDefault(-1),
						destroyContentOnMeltdown     = destroyTriState,
						defaultEnrichmentStep        = args.GetArgument<int>("defaultEnrichmentStep").ElseDefault(-1),
						steps                        = steps,
					};
				}),

			#endregion

			#region Define port reference for machine port editing / cloning

			// Mirrors the Python `Port(...)` factory. Builds a plain struct that
			// edit_machine_ports / build_machine later turn into an IoPortTemplate via
			// buildIoPortTemplate. Validation (shape exists, direction is one of ±X/±Y,
			// type is input/output) is deferred to that turn-into step so a bad spec
			// fails at the call site that consumes it, not here.
			["Port"] = new Constructor(
				["name", "type", "shape", "position", "direction", "canOnlyConnectToTransports"],
				(args) => {
					string nameStr = args.GetArgument<string>("name").ElseRequiredThrow();
					if (nameStr.Length != 1) {
						throw new ArgumentException(
							$"Port.name must be a single character (got '{nameStr}', length={nameStr.Length}).");
					}
					// The When<(int,int,int)> chain that used to live here
					// works for the literal `(6, 5, 0)` tuple shape but
					// silently rejected long-typed literals and List-shaped
					// values. Use a single parser that handles every shape
					// the Python lexer can produce + emits a readable error
					// when it can't, so a bad position arg points the
					// modder straight at the call site.
					Vector3i pos = parsePortPositionArg(args["position"]?.Value);
					return new Port {
						name = nameStr[0],
						type = args.GetArgument<string>("type").ElseRequiredThrow(),
						shape = args.GetArgument<string>("shape").ElseRequiredThrow(),
						position = pos,
						direction = args.GetArgument<string>("direction").ElseRequiredThrow(),
						canOnlyConnectToTransports = args.GetArgument<bool>("canOnlyConnectToTransports")
							.ElseDefault(false),
					};
				}),

			#endregion

			#region Build recipe

			["build_recipe"] = new Constructor([
				"recipeId",
				"name",
				"description",
				"machine",
				"research",
				"duration",
				"ingredients",
				"products",
				"power"
			], (args) => {
				MachineProto machine = args.GetArgument<MachineProto>("machine")
					.When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(id)))
					.ElseRequiredThrow();
				RecipeProtoBuilder.State builder = registrator.RecipeProtoBuilder
					.Start(
							name: args.GetArgument<string>("name").ElseRequiredThrow(),
							recipeId:
							args.GetArgument<RecipeProto.ID>("recipeId")
								.When<string>(v => new RecipeProto.ID(v))
								.ElseRequiredThrow(),
							machine: machine
						);

				if (args.GetArgument<string>("description").WhenExists(out string description)) {
					builder = builder.Description(description);
				}

				if (args.GetArgument<List<object>>("ingredients").WhenExists(out var ingredientsList)) {
					ingredientsList.Select(e => (Product)e)
						.Call(e => builder = builder.AddInput(
								portSelector: e.port,
								product: e.product,
								quantity: e.quantity
							))
						.Loop(); // invoke ling actions
				}

				if (args.GetArgument<List<object>>("products").WhenExists(out var productsList)) {
					PortEntry[] ports = machine.Ports
						.Where(p => p.Spec.Type == IoPortType.Output)
						.Select(p => new PortEntry(p.Name, p.Shape.AllowedProductType))
						.ToArray();
					productsList.Select(e => (Product)e)
						.Call(e => builder = builder.AddOutput(
								portSelector: e.port == "VIRTUAL" ? e.port
									: e.port != "*" && e.port != "VIRTUAL"
									? ports
										.Where(p => p.Name == e.port)
										.Where(p => p.Type == e.product.Type)
										.Where(p => !p.Used)
										.Select(p => {
											p.Used = true;
											return p.Name;
										})
										.FirstOrDefault()
									?? throw new ArgumentException($"Port '{e.port}' is already used")
									: ports
										.Where(p => p.Type == e.product.Type)
										.Where(p => !p.Used)
										.Select(p => {
											p.Used = true;
											return p.Name;
										})
										.FirstOrDefault()
									?? throw new ArgumentException(
										$"Cannot get empty port for product: {e.product.Id.Value}"),
								product: e.product,
								quantity: e.quantity
							))
						.Loop(); // invoke ling actions
				}

				builder.SetDuration(args.GetArgument<Duration>("duration").When<int>(Duration.FromSec)
					.ElseDefault(Duration.FromSec(60)));

				if (args.GetArgument<Percent>("power")
					.When<int>(i => i.Percent())
					.WhenExists(out Percent power)) {
					builder.SetPowerMultiplier(power);
				}

				RecipeProto recipe = builder.BuildAndAdd();
				if (args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
					.When<string>(id
						=> registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
					.WhenExists(out ResearchNodeProto research)) {
					typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
						.SetValue(research, research.Units
							.AsEnumerable()
							.Concat(new IUnlockNodeUnit[] {
								new RecipeUnlock(recipe, machine, false, true),
								//new ProtoWithIconUnlock(machine, false)
							})
							//.Distinct(i => {
							//	if (i is ProtoWithIconUnlock protoUnlock) {
							//		return protoUnlock.Proto.Id.Value;
							//	}
							//	return DateTime.Now.Ticks.ToString();
							//})
							.ToImmutableArray());

					typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
							BindingFlags.NonPublic | BindingFlags.Instance)
						.SetValue(research.Graphics, research.Graphics.IconsProtos
							.AsEnumerable()
							.Concat([machine])
							.Distinct()
							.ToImmutableArray());

					_ = builder.SetAsLockedOnInit();
				}

				return recipe;
			}),

			#endregion

			#region Edit recipe

			["edit_recipe"] = new Constructor([
				"recipe",
				"machine",
				"research",
				"duration",
				"ingredients",
				"products",
				"power"
			], (args) => {
				RecipeProto recipe = args.GetArgument<RecipeProto>("recipe")
					.When<RecipeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<RecipeProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<RecipeProto>(new RecipeProto.ID(id)))
					.ElseRequiredThrow();

				if (args.GetArgument<List<object>>("ingredients").WhenExists(out var ingredientsList)) {
					ingredientsList.Select(e => (Product)e)
						.Call(e => {
							RecipeInput recipeInput = recipe.AllInputs
								.AsEnumerable()
								.FirstOrDefault(a => a.Product.Id == e.product.Id);

							if (recipeInput == null) {
								throw new ArgumentException("Recipe has no ingredient with id: "
									+ e.product.Id.Value);
							}

							typeof(RecipeInput)
								.GetField("Quantity", BindingFlags.Public | BindingFlags.Instance)
								?.SetValue(recipeInput, e.quantity);
						})
						.Loop(); // invoke ling actions
				}

				if (args.GetArgument<List<object>>("products").WhenExists(out var productsList)) {
					productsList.Select(e => (Product)e)
						.Call(e => {
							RecipeOutput recipeOutput = recipe.AllOutputs
								.AsEnumerable()
								.FirstOrDefault(a => a.Product.Id == e.product.Id);

							if (recipeOutput == null) {
								throw new ArgumentException("Recipe has no produc with id: " + e.product.Id.Value);
							}

							typeof(RecipeOutput)
								.GetField("Quantity", BindingFlags.Public | BindingFlags.Instance)
								?.SetValue(recipeOutput, e.quantity);
						})
						.Loop(); // invoke ling actions
				}

				if (args.GetArgument<Duration>("duration")
					.When<int>(Duration.FromSec)
					.WhenExists(out Duration duration)) {
					typeof(RecipeProto)
						.GetField("<Duration>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(recipe, duration);
				}

				if (args.GetArgument<Percent>("power")
					.When<int>(i => i.Percent())
					.WhenExists(out Percent power)) {
					typeof(RecipeProto)
						.GetField("PowerMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(recipe, power);
				}

				if (args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
					.When<string>(id
						=> registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
					.WhenExists(out ResearchNodeProto research)) {
					MachineProto machine = args.GetArgument<MachineProto>("machine")
						.When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
						.When<string>(id
							=> registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(id)))
						.ElseRequiredThrow();

					if (!machine.Recipes.Any(r => r.Id == recipe.Id)) {
						machine.AddRecipe(recipe);
					}

					typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(research, research.Units
							.AsEnumerable()
							.Concat([
								new RecipeUnlock(recipe, machine, false, true),
								//new ProtoWithIconUnlock(machine, false)
							])
							//.Distinct(i => {
							//	if (i is ProtoWithIconUnlock protoUnlock) {
							//		return protoUnlock.Proto.Id.Value;
							//	}
							//	return DateTime.Now.Ticks.ToString();
							//})
							.ToImmutableArray());

					typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
							BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(research.Graphics, research.Graphics.IconsProtos
							.AsEnumerable()
							.Concat([machine])
							.ToImmutableArray());
				}

				return recipe;
			}),

			#endregion

			#region Build edict

			["build_edict"] = new Constructor([
				"edictId",
				"name",
				"description",
				"category",
				"icon",
				"implementation",
				"cost",
				"isGeneratingUnity",
				"previousTier",
			], (args) => {
				Proto.ID edictId = args.GetArgument<EdictProto.ID>("edictId")
					.When<string>(v => new EdictProto.ID(v))
					.ElseRequiredThrow();
				return registrator.PrototypesDb.Add(new EdictProto(
						id: edictId,
						strings: Proto.CreateStr(
								edictId,
								name: args.GetArgument<string>("name").ElseRequiredThrow(),
								descShort: args.GetArgument<string>("description").ElseDefault("")
							),
						category: args.GetArgument<EdictCategoryProto>("category")
							.When<EdictCategoryProto.ID>(id
								=> registrator.PrototypesDb.GetOrThrow<EdictCategoryProto>(id))
							.When<string>(id
								=> registrator.PrototypesDb.GetOrThrow<EdictCategoryProto>(
									new EdictCategoryProto.ID(id)))
							.ElseRequiredThrow(),
						monthlyUpointsCost: args.GetArgument<Upoints>("cost")
							.When<int>(i => i.Upoints())
							.ElseDefault(0.3.Upoints()),
						edictImplementation: args.GetArgument<Type>("implementation").ElseRequiredThrow(),
						graphics: new EdictProto.Gfx(ResolveIconPath(args.GetArgument<Tex>("icon")
							.When<string>(s => new Tex() { path = s })
							.ElseRequiredThrow().path)),
						isGeneratingUnity: args.GetArgument<bool?>("isGeneratingUnity").ElseDefault(null),
						previousTier: args.GetArgument<Option<EdictProto>>("previousTier")
							.When<EdictProto>(Option.Some)
							.When<EdictProto.ID>(id => registrator.PrototypesDb.GetOrThrow<EdictProto>(id))
							.When<string>(id
								=> registrator.PrototypesDb.GetOrThrow<EdictProto>(new EdictProto.ID(id)))
							.ElseDefault(Option.None)
					));
			}),

			#endregion

			#region Unlock

			["add_unlock_recipe"] = new Constructor(["research", "machine", "recipe"], (args) => {
				RecipeProto recipe = args.GetArgument<RecipeProto>("recipe")
					.When<RecipeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<RecipeProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<RecipeProto>(new RecipeProto.ID(id)))
					.ElseRequiredThrow();
				MachineProto machine = args.GetArgument<MachineProto>("machine")
					.When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(id)))
					.ElseRequiredThrow();
				ResearchNodeProto research = args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
					.When<string>(id
						=> registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
					.ElseRequiredThrow();

				typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
					.SetValue(research, research.Units
						.AsEnumerable()
						.Concat(new IUnlockNodeUnit[] {
							new RecipeUnlock(recipe, machine, false, true),
							new ProtoWithIconUnlock(machine, false)
						})
						.Distinct(i => {
							Thread.Sleep(1);
							return i is ProtoWithIconUnlock protoUnlock
								? protoUnlock.Proto.Id.Value
								: DateTime.Now.Ticks.ToString();
						})
						.ToImmutableArray());

				typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
						BindingFlags.NonPublic | BindingFlags.Instance)
					.SetValue(research.Graphics, research.Graphics.IconsProtos
						.AsEnumerable()
						.Concat([machine])
						.ToImmutableArray());
				return null;
			}),

			["add_unlock_product"] = new Constructor(["research", "product"], (args) => {
				ProductProto product = args.GetArgument<ProductProto>("product")
					.When<ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ProductProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<ProductProto>(new ProductProto.ID(id)))
					.ElseRequiredThrow();
				ResearchNodeProto research = args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
					.When<string>(id
						=> registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
					.ElseRequiredThrow();

				typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
					.SetValue(research, research.Units
						.AsEnumerable()
						.Concat(new IUnlockNodeUnit[] { new ProductUnlock(product, false) })
						.ToImmutableArray());
				return null;
			}),

			["add_unlock_machine"] = new Constructor(["research", "machine"], (args) => {
				MachineProto machine = args.GetArgument<MachineProto>("machine")
					.When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(id)))
					.ElseRequiredThrow();

				ResearchNodeProto research = args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
					.When<string>(id
						=> registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
					.ElseRequiredThrow();

				typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
					?.SetValue(research, research.Units
						.AsEnumerable()
						.Concat([new ProtoWithIconUnlock(machine, false)])
						.Distinct(i => i is ProtoWithIconUnlock protoUnlock
							? protoUnlock.Proto.Id.Value
							: DateTime.Now.Ticks.ToString())
						.ToImmutableArray());

				typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
						BindingFlags.NonPublic | BindingFlags.Instance)
					?.SetValue(research.Graphics, research.Graphics.IconsProtos
						.AsEnumerable()
						.Concat([machine])
						.ToImmutableArray());
				return null;
			}),

			#endregion

			#region Build product

			["build_product_loose"] = new Constructor([
				"productId", "name", "description", "icon", "color", "particleColor", "isDumped",
				"isStorable", "isRecyclable", "material", "isWaste", "isRough", "isLocked",
				"pinToHomeScreen", "maxTransport", "prefabPath", "dumpsAs", "research"
			], (args) => {
				var id = args.GetArgument<ProductProto.ID>("productId")
					.When<string>(ids => new ProductProto.ID(ids))
					.ElseRequiredThrow();
				var name = args.GetArgument<string>("name").ElseRequiredThrow();
				var icon = ResolveIconPath(args.GetArgument<Tex>("icon")
					.When<string>(s => new Tex { path = s })
					.ElseRequiredThrow()
					.path);
				var material = args.GetArgument<Mat>("material")
					.When<string>(s => new Mat { path = s })
					.ElseRequiredThrow();
				var desc = args.GetArgument<string>("description").ElseDefault("");
				var isDumped = args.GetArgument<bool>("isDumped").ElseDefault(false);
				var isStorable = args.GetArgument<bool>("isStorable").ElseDefault(false);
				var isRecyclable = args.GetArgument<bool>("isRecyclable").ElseDefault(false);
				var isWaste = args.GetArgument<bool>("isWaste").ElseDefault(false);
				var isRough = args.GetArgument<bool>("isRough").ElseDefault(false);
				var pinToHome = args.GetArgument<bool>("pinToHomeScreen").ElseDefault(false);
				// Optional. Only used as resourcesVizColor (resource overlay). Pile appearance
				// itself comes from the material/texture, so a missing color is fine.
				var color = args.GetArgument<ColorRgba>("color")
					.When<(int r, int g, int b)>(t => new ColorRgba((byte)t.r, (byte)t.g, (byte)t.b))
					.ElseDefault(ColorRgba.White);
				// Optional. When omitted, LooseProductMaterialManager auto-derives the particle
				// color from the average of the pile's albedo texture. Pass this to override that
				// (useful when our custom albedo's average doesn't match the desired particle hue).
				ColorRgba? particleColor = null;
				if (args.GetArgument<ColorRgba>("particleColor")
					.When<(int r, int g, int b)>(t => new ColorRgba((byte)t.r, (byte)t.g, (byte)t.b))
					.WhenExists(out ColorRgba particleColorVal)) {
					particleColor = particleColorVal;
				}
				// Optional. Override the pile prefab path. Default picks rough/smooth based on isRough.
				string prefabPath = args.GetArgument<string>("prefabPath").ElseDefault(
					isRough
						? "Assets/Base/Transports/ConveyorLoose/PileRough.prefab"
						: "Assets/Base/Transports/ConveyorLoose/PileSmooth.prefab");
				// Optional. Default of 5 inherited from LooseProductProto when omitted.
				Quantity? maxTransport = null;
				if (args.GetArgument<Quantity>("maxTransport")
					.When<int>(i => new Quantity(i))
					.WhenExists(out Quantity mt)) {
					maxTransport = mt;
				}

				var product = new LooseProductProto(
						id: id,
						strings: Proto.CreateStr(id, name, desc),
						graphics: new LooseProductProto.Gfx(
								prefabPath: prefabPath,
								pileMaterialAssetPath: material.path,
								useRoughPileMeshes: isRough,
								resourcesVizColor: color,
								particleColor: particleColor,
								customIconPath: icon
							),
						isDumpedOnTerrainByDefault: isDumped,
						isStorable: isStorable,
						isRecyclable: isRecyclable,
						isWaste: isWaste,
						pinToHomeScreenByDefault: pinToHome,
						maxQuantityPerTransportedProduct: maxTransport
					);

				// Optional terrain-dump wiring. `LooseProductProto.IsDumpedOnTerrainByDefault`
				// alone is NOT enough — the engine also requires a `LooseProductParam` proto-param
				// pointing at a `TerrainMaterialProto` so it knows what terrain to transform the
				// dumped pile into. Without it, `CanBeOnTerrain` stays false and the in-game dump
				// option is missing. dumpsAs = Ids.TerrainMaterials.Slag etc., or a string id.
				if (args.GetArgument<Mafi.Core.Prototypes.Proto.ID>("dumpsAs")
					.When<string>(s => new Mafi.Core.Prototypes.Proto.ID(s))
					.WhenExists(out Mafi.Core.Prototypes.Proto.ID dumpsAsId)) {
					product.AddParam(new Mafi.Core.Buildings.Farms.LooseProductParam(dumpsAsId));
				}

				// Resolve optional research first so it can both lock the product on
				// init (default for research-gated products) and append the product
				// to the research's Units list as a ProductUnlock.
				ResearchNodeProto researchLoose = args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
					.ElseNull();

				bool lockedLoose = args.GetArgument<bool>("isLocked").ElseDefault(researchLoose != null);
				registrator.PrototypesDb.Add(product, lockedLoose);

				if (researchLoose != null) {
					typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(researchLoose, researchLoose.Units
							.AsEnumerable()
							.Concat(new IUnlockNodeUnit[] { new ProductUnlock(product, false) })
							.ToImmutableArray());
				}
				return product;
			}),

			["build_product_unit"] = new Constructor([
				"productId", "name", "icon", "prefab", "maxTransport", "packingMode",
				"allowPackingNoise", "rotateSecondPackedItem90Degs",
				"description", "isStorable", "isWaste", "isLocked", "research"
			], (args) => {
				var id = args.GetArgument<ProductProto.ID>("productId")
					.When<string>(ids => new ProductProto.ID(ids))
					.ElseRequiredThrow();
				var name = args.GetArgument<string>("name").ElseRequiredThrow();
				var icon = ResolveIconPath(args.GetArgument<Tex>("icon")
					.When<string>(s => new Tex { path = s })
					.ElseRequiredThrow()
					.path);
				var prefab = args.GetArgument<string>("prefab")
					.When<Prefab>(p => p.path)
					.ElseRequiredThrow();
				var desc = args.GetArgument<string>("description").ElseDefault("");
				var maxTransport = args.GetArgument<Quantity>("maxTransport")
					.When<int>(i => new Quantity(i))
					.ElseDefault(new Quantity(3));
				var packing = args.GetArgument<CountableProductStackingMode>("packingMode")
					.When<string>(s => (CountableProductStackingMode)Enum.Parse(typeof(CountableProductStackingMode), s, ignoreCase: true))
					.ElseDefault(CountableProductStackingMode.Auto);
				var allowPackingNoise = args.GetArgument<bool>("allowPackingNoise").ElseDefault(false);
				var rotateSecondPacked = args.GetArgument<bool>("rotateSecondPackedItem90Degs").ElseDefault(false);
				var isStorable = args.GetArgument<bool>("isStorable").ElseDefault(false);
				var isWaste = args.GetArgument<bool>("isWaste").ElseDefault(false);

				var product = new CountableProductProto(
						id: id,
						strings: Proto.CreateStr(id, name, desc),
						maxQuantityPerTransportedProduct: maxTransport,
						isStorable: isStorable,
						graphics: new CountableProductProto.Gfx(
								prefabPath: prefab,
								customIconPath: icon,
								packingMode: packing,
								allowPackingNoise: allowPackingNoise,
								rotateSecondPackedItem90Degs: rotateSecondPacked
							),
						isWaste: isWaste
					);
				// Optional research: lock product on init (default when research is set)
				// and append a ProductUnlock so the research node displays it.
				ResearchNodeProto researchUnit = args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
					.When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
					.ElseNull();

				bool lockedUnit = args.GetArgument<bool>("isLocked").ElseDefault(researchUnit != null);
				registrator.PrototypesDb.Add(product, lockedUnit);

				if (researchUnit != null) {
					typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(researchUnit, researchUnit.Units
							.AsEnumerable()
							.Concat(new IUnlockNodeUnit[] { new ProductUnlock(product, false) })
							.ToImmutableArray());
				}
				return product;
			}),

			["build_product_fluid"] = new Constructor([
				"productId", "name", "icon", "canBeDiscarded", "packingMode", "allowPackingNoise",
				"description",
				"isStorable", "isWaste", "isLocked", "color", "transportColor", "transportAccentColor"
			], (args) => {
				var id = args.GetArgument<ProductProto.ID>("productId")
					.When<string>(ids => new ProductProto.ID(ids))
					.ElseRequiredThrow();
				var name = args.GetArgument<string>("name").ElseRequiredThrow();
				var icon = ResolveIconPath(args.GetArgument<Tex>("icon")
					.When<string>(s => new Tex { path = s })
					.ElseRequiredThrow()
					.path);
				var desc = args.GetArgument<string>("description").ElseDefault("");
				var maxTransport = args.GetArgument<Quantity>("maxTransport")
					.When<int>(i => new Quantity(i))
					.ElseDefault(new Quantity(3));
				var packing = args.GetArgument<CountableProductStackingMode>("packingMode")
					.ElseDefault(CountableProductStackingMode.Auto);
				var allowPackingNoise = args.GetArgument<bool>("allowPackingNoise").ElseDefault(false);
				var isStorable = args.GetArgument<bool>("isStorable").ElseDefault(false);
				var isWaste = args.GetArgument<bool>("isWaste").ElseDefault(false);
				var canBeDiscarded = args.GetArgument<bool>("canBeDiscarded").ElseDefault(true);

				// Accept either a ColorRgba instance or a Python tuple `(r, g, b)`
				// — matches build_product_loose's color/particleColor handling
				// so modders don't need to remember which call uses which form.
				var color = args.GetArgument<ColorRgba>("color")
					.When<(int r, int g, int b)>(t => new ColorRgba((byte)t.r, (byte)t.g, (byte)t.b))
					.ElseDefault(default);
				var transportColor = args.GetArgument<ColorRgba>("transportColor")
					.When<(int r, int g, int b)>(t => new ColorRgba((byte)t.r, (byte)t.g, (byte)t.b))
					.ElseDefault(default);
				var transportAccentColor = args.GetArgument<ColorRgba>("transportAccentColor")
					.When<(int r, int g, int b)>(t => new ColorRgba((byte)t.r, (byte)t.g, (byte)t.b))
					.ElseDefault(default);

				var product = new FluidProductProto(
						id: id,
						strings: Proto.CreateStr(id, name, desc),
						isStorable: isStorable,
						canBeDiscarded: canBeDiscarded,
						graphics: new FluidProductProto.Gfx(
								prefabPath: Option.None,
								customIconPath: icon,
								color: color,
								transportColor: transportColor,
								transportAccentColor: transportAccentColor
							),
						isWaste: isWaste
					);
				registrator.PrototypesDb.Add(product, args.GetArgument<bool>("isLocked").ElseDefault(false));
				return product;
			}),

			#endregion

			#region Add Machine

			["add_machine"] = new Constructor([
				"name", // str
				"entityId", // str, MachineProto.ID, StaticEntityProto.ID, Proto.ID
				"layout", // list(string)
				"prefabPath", // str # from AssetBundle

				// OPTIONALS
				"customIconPath", // str, Tex
				"description", // str
				"lockedOnInit", // bool = False

				// OPTIONALS TOOLING
				"translationName",
				"translationDescription",
			], args => {
				MachineProtoBuilder.MachineProtoBuilderStateBase builder =
					args.GetArgument<string>("translationDescription")
						.WhenExists(out string translationName)
						? registrator.MachineProtoBuilder.Start(
							args.GetArgument<string>("name").ElseRequiredThrow(),
							args.GetArgument<MachineProto.ID>("entityId")
								.When<Proto.ID>(s => new MachineProto.ID(s.Value))
								.When<StaticEntityProto.ID>(s => new MachineProto.ID(s.Value))
								.When<string>(s => new MachineProto.ID(s))
								.ElseRequiredThrow(),
							translationName)
						: registrator.MachineProtoBuilder.Start(
							args.GetArgument<string>("name").ElseRequiredThrow(),
							args.GetArgument<MachineProto.ID>("entityId")
								.When<Proto.ID>(s => new MachineProto.ID(s.Value))
								.When<StaticEntityProto.ID>(s => new MachineProto.ID(s.Value))
								.When<string>(s => new MachineProto.ID(s))
								.ElseRequiredThrow());

				if (args.GetArgument<string>("description")
					.WhenExists(out string description)) {

					builder =
						args.GetArgument<string>("translationDescription")
							.WhenExists(out string translationDescription)
							? builder.Description(description, translationDescription)
							: builder.Description(description);
				}

				string[] layout = args.GetArgument<List<string>>("layout")
					.When<List<object>>(l => [.. l.ElementsToString()])
					.ElseRequiredThrow()
					.ToArray();

				EntityLayoutParams layoutParams =
					args.GetArgument<EntityLayoutParams>("layoutParams")
						.ElseDefault(EntityLayoutParams.DEFAULT);

				builder = builder.SetLayout(layoutParams, layout);
				builder = builder.SetPrefabPath(args.GetArgument<string>("prefabPath").ElseRequiredThrow());

				if (args.GetArgument<Tex>("customIconPath")
					.When<string>(s => new Tex() { path = s })
					.WhenExists(out Tex customIconPath)) {
					builder = builder.SetCustomIconPath(customIconPath.path);
				}

				if (args.GetArgument<bool>("lockedOnInit").ElseDefault(false)) {
					builder = builder.SetAsLockedOnInit();
				}

				return builder.BuildAndAdd();
			}),

			#endregion

			#region build_generator

			// Register a new ElectricityGeneratorFromProductProto by cloning an existing
			// vanilla generator (DieselGeneratorT2 by default) and overriding the fuel
			// mapping. ElectricityGeneratorFromProductProto has ONE built-in InputProduct
			// / OutputProduct / OutputElectricity / Duration tuple per instance — it isn't
			// recipe-list-driven — so each custom fuel needs its own generator instance.
			//
			// We clone non-customizable plumbing (layout, costs, graphics, animation,
			// destroyReason, electricity-proto reference) from the source generator and
			// substitute our own input/output/electricity/duration.
			["build_generator"] = new Constructor([
				"id",                          // str — required, new generator id
				"name",                        // str — required, display name
				"description",                 // str — optional
				"source",                      // str — id of generator to clone from, default "DieselGeneratorT2"
				"inputProduct",                // Product — required, the fuel
				"outputProduct",               // Product — optional, waste byproduct
				"outputElectricityKw",         // int — required, kW generated per cycle
				"duration",                    // Duration | int — optional, cycle time (default: source's)
				"generationPriority",          // int — optional (default: source's)
				"bufferCapacityMultiplier",    // int — optional (default: source's)
				"research",                    // ResearchNodeProto | ID | str — optional
				"lockedOnInit"                 // bool — optional (default: research != null)
			], args => {
				// Required-arg sanity checks up front — see edit_machine_ports
				// for the rationale. The AnyArgument chain's empty
				// "received: " trailer when value is None is opaque; a
				// readable error pointing at the specific arg lets the
				// modder fix the draft directly. Mirrored on the editor
				// side via GeneratorDef.MissingMandatoryFields so a
				// freshly-added build_generator without inputProduct
				// never hits this path at game load.
				if (args["id"]?.Value == null) {
					throw new ArgumentException(
						"build_generator: required argument `id` is missing or None.");
				}
				if (args["inputProduct"]?.Value == null) {
					throw new ArgumentException(
						"build_generator: required argument `inputProduct` is missing or None. " +
						"Pass a Product(\"<fuelProductId>\", Quantity(<n>)) call.");
				}
				if (args["outputElectricityKw"]?.Value == null) {
					throw new ArgumentException(
						"build_generator: required argument `outputElectricityKw` is missing or None. " +
						"Pass a positive integer (kW produced per cycle).");
				}
				string id = args.GetArgument<string>("id")
					.When<Mafi.Core.Prototypes.Proto.ID>(p => p.Value)
					.ElseRequiredThrow();
				string name = args.GetArgument<string>("name").ElseRequiredThrow();
				string desc = args.GetArgument<string>("description").ElseDefault("");
				DiagnosticTrace.Step($"build_generator[{id}]: start (name='{name}')");

				// 1) Locate the source generator (template).
				string sourceIdStr = args.GetArgument<string>("source")
					.When<Mafi.Core.Prototypes.Proto.ID>(p => p.Value)
					.ElseDefault("DieselGeneratorT2");

				var sourceProto = registrator.PrototypesDb
					.All<Mafi.Base.Prototypes.Machines.PowerGenerators.ElectricityGeneratorFromProductProto>()
					.FirstOrDefault(p => p.Id.Value == sourceIdStr);
				if (sourceProto == null) {
					throw new ArgumentException(
						$"build_generator: source generator '{sourceIdStr}' not found " +
						"in PrototypesDb (must be an ElectricityGeneratorFromProductProto).");
				}

				// 2) Required fuel. Product.product is already a resolved ProductProto
				//    (the build_recipe Product class enforces that).
				Product inputProductArg = args.GetArgument<Product>("inputProduct").ElseRequiredThrow();
				ProductQuantity inputProductQuantity = new ProductQuantity(
					inputProductArg.product, inputProductArg.quantity);

				// 3) Optional waste byproduct (Nullable<ProductQuantity>).
				ProductQuantity? outputProductQuantity = null;
				if (args.GetArgument<Product>("outputProduct").WhenExists(out Product outArg)) {
					outputProductQuantity = new ProductQuantity(outArg.product, outArg.quantity);
				}

				// 4) Numerics. Defaults come from the source so missing args still produce
				//    a sensible machine (e.g. a "rename only" clone).
				int electricityKw = args.GetNumberArgument<int>("outputElectricityKw")
					.ElseDefault((int)sourceProto.OutputElectricity.Value);
				Duration duration = args.GetArgument<Duration>("duration")
					.When<int>(Duration.FromSec)
					.ElseDefault(sourceProto.Duration);
				int genPriority = args.GetNumberArgument<int>("generationPriority")
					.ElseDefault(sourceProto.GenerationPriority);
				int bufferCap = args.GetNumberArgument<int>("bufferCapacityMultiplier")
					.ElseDefault(sourceProto.BufferCapacityMultiplier);

				// 5) The virtual Electricity product. Reuse the source's reference so we
				//    don't have to know the literal id of Product_Virtual_Electricity here.
				FieldInfo electricityProtoField = sourceProto.GetType()
					.GetField("electricityProto", BindingFlags.NonPublic | BindingFlags.Instance)
					?? sourceProto.GetType()
						.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
						.FirstOrDefault(f => f.FieldType == typeof(ProductProto)
							&& f.Name.IndexOf("electric", StringComparison.OrdinalIgnoreCase) >= 0);
				ProductProto electricityProto = electricityProtoField != null
					? (ProductProto)electricityProtoField.GetValue(sourceProto)
					: registrator.PrototypesDb.GetOrThrow<ProductProto>(new ProductProto.ID("Product_Virtual_Electricity"));

				// 6) Resolve port shapes for the new input/output products. These must be
				//    baked into the LAYOUT before the ctor runs — the ctor validates port
				//    shapes against the inputProduct/outputProduct it receives, so a layout
				//    still carrying the source's pipe-shaped ports (when our products are
				//    unit) makes the ctor throw "No output port found for ...".
				var inputShape = portShapeForProduct(registrator.PrototypesDb, inputProductQuantity.Product);
				var outputShape = outputProductQuantity.HasValue
					? portShapeForProduct(registrator.PrototypesDb, outputProductQuantity.Value.Product)
					: null;
				DiagnosticTrace.Step($"build_generator[{id}]: port shapes resolved (in={inputShape.Id.Value}, out={(outputShape != null ? outputShape.Id.Value : "(keep)")})");

				// Build a new layout from the source's SourceLayoutStr by substituting each
				// port's existing IoPortShape LayoutChar with the LayoutChar of the shape we
				// actually want for that port. Then re-parse via the live EntityLayoutParser
				// so all derived state (tiles, port positions, etc.) stays internally
				// consistent — much cleaner than reflection-cloning the EntityLayout and
				// patching its Ports field.
				var srcLayout = sourceProto.Layout;
				var charSubstitutions = new Dictionary<char, char>();
				foreach (var srcPort in srcLayout.Ports) {
					char oldChar = srcPort.Spec.Shape.LayoutChar;
					Mafi.Core.Ports.Io.IoPortShapeProto newShape;
					switch (srcPort.Spec.Type) {
						case Mafi.IoPortType.Input:  newShape = inputShape;                       break;
						case Mafi.IoPortType.Output: newShape = outputShape ?? srcPort.Spec.Shape; break;
						default:                     newShape = srcPort.Spec.Shape;               break;
					}
					char newChar = newShape.LayoutChar;
					if (oldChar == newChar) continue;
					if (charSubstitutions.TryGetValue(oldChar, out char prior) && prior != newChar) {
						// Same LayoutChar would have to map to two different new shapes — happens
						// when the source's input and output use the same shape but we want
						// different shapes for them. Layout-string substitution can't disambiguate
						// in that case. For our typical pattern (both ports same shape in source,
						// both go to the same new shape) this doesn't trigger.
						Log.Warning($"build_generator[{id}]: ambiguous LayoutChar substitution for '{oldChar}' " +
							$"(both '{prior}' and '{newChar}' requested) — pick a source generator " +
							$"whose I/O port shapes differ if both directions need different new shapes.");
						continue;
					}
					charSubstitutions[oldChar] = newChar;
				}

				string newLayoutSrc = srcLayout.SourceLayoutStr;
				foreach (var kv in charSubstitutions) {
					newLayoutSrc = newLayoutSrc.Replace(kv.Key, kv.Value);
				}
				string[] layoutLines = sanitizeLayoutLines(newLayoutSrc);
				var layoutClone = registrator.LayoutParser.ParseLayoutOrThrow(srcLayout.LayoutParams, layoutLines);
				DiagnosticTrace.Step($"build_generator[{id}]: layout parsed (substitutions={charSubstitutions.Count}, " +
					$"ports={layoutClone.Ports.Length})");

				// 7) Construct the new proto directly. The layout we built above already
				//    carries the right port shapes, so the ctor's port-validation passes.
				DiagnosticTrace.Step($"build_generator[{id}]: invoking ctor (source={sourceIdStr}, kw={electricityKw})");
				var newProto = new Mafi.Base.Prototypes.Machines.PowerGenerators.ElectricityGeneratorFromProductProto(
					new StaticEntityProto.ID(id),
					Mafi.Core.Prototypes.Proto.CreateStr(
						new StaticEntityProto.ID(id),
						name, desc, /*translationComment*/ null),
					layoutClone,
					sourceProto.Costs,
					Mafi.Electricity.FromKw(electricityKw),
					genPriority,
					inputProductQuantity,
					outputProductQuantity,
					electricityProto,
					bufferCap,
					duration,
					sourceProto.ProductDestroyReason,
					sourceProto.AnimationParams,
					sourceProto.Graphics);
				DiagnosticTrace.Step($"build_generator[{id}]: ctor returned");

				// 8) Optional research wiring (lock + append unlock).
				ResearchNodeProto research = args.GetArgument<ResearchNodeProto>("research")
					.When<ResearchNodeProto.ID>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(rid))
					.When<string>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(rid)))
					.ElseNull();

				bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
				DiagnosticTrace.Step($"build_generator[{id}]: PrototypesDb.Add (locked={locked})");
				registrator.PrototypesDb.Add(newProto, locked);
				DiagnosticTrace.Step($"build_generator[{id}]: Add completed");

				if (research != null) {
					DiagnosticTrace.Step($"build_generator[{id}]: wiring research unlock ({research.Id.Value})");
					typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(research, research.Units
							.AsEnumerable()
							.Concat(new IUnlockNodeUnit[] { new ProtoWithIconUnlock(newProto, false) })
							.ToImmutableArray());
					// Match the build_recipe pattern: append the new proto to IconsProtos
					// via .AsEnumerable() (Mafi's ImmutableArray doesn't directly implement
					// System.Collections.Generic.IEnumerable<T>).
					typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
							BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(research.Graphics, research.Graphics.IconsProtos
							.AsEnumerable()
							.Concat(new[] { newProto })
							.Distinct()
							.ToImmutableArray());
				}

				DiagnosticTrace.Step($"build_generator[{id}]: complete");
				return newProto;
			}),

			#endregion

			#region add_toolbar_category

			["add_toolbar_category"] = new Constructor([
				"categoryId", // Proto.ID, str
				"name", // name
				"icon", // texture or path to texture
				"parent", // ToolbarCategoryProto | Proto.ID | str
				"machines", // list[StaticEntityProto | StaticEntityProto.ID | str]
			], args => {
				Proto.ID id = args.GetArgument<Proto.ID>("categoryId")
					.When<string>(s => new Proto.ID(s))
					.ElseRequiredThrow();

				string name = args.GetArgument<string>("name")
					.ElseRequiredThrow();

				string icon = ResolveIconPath(args.GetArgument<Tex>("icon")
					.When<string>(s => new Tex { path = s })
					.ElseRequiredThrow()
					.path);

				ToolbarCategoryProto parent = args.GetArgument<ToolbarCategoryProto>("parent")
					.When<Proto.ID>(s => registrator.PrototypesDb.GetOrThrow<ToolbarCategoryProto>(s))
					.When<string>(s => registrator.PrototypesDb.GetOrThrow<ToolbarCategoryProto>(new Proto.ID(s)))
					.ElseNull();

				ToolbarCategoryProto category = registrator.PrototypesDb.Add(new ToolbarCategoryProto(
					id,
					order: args.GetNumberArgument<float>("order")
						.ElseDefault(10),
					strings: Proto.CreateStr(id, name),
					iconPath: icon,
					parentCategory: parent));

				if (args.GetArgument<List<object>>("entities")
					.WhenExists(out var entitiesList)) {
					foreach (object entityAsObj in entitiesList) {
						switch (entityAsObj) {
						case ILayoutEntityProto lep: lep.Graphics.ExtendCategories(category); break;
						case MachineProto.ID machineID: registrator.PrototypesDb
								.GetOrThrow<MachineProto>(machineID)
								.Graphics.ExtendCategories(category);
							break;
						case StaticEntityProto.ID entityID:
							registrator.PrototypesDb
								.GetOrThrow<ILayoutEntityProto>(entityID)
								.Graphics.ExtendCategories(category);
							break;
						case Proto.ID protoID: registrator.PrototypesDb
								.GetOrThrow<ILayoutEntityProto>(protoID)
								.Graphics.ExtendCategories(category);
							break;
						case string strID: registrator.PrototypesDb
								.GetOrThrow<ILayoutEntityProto>(new Proto.ID(strID))
								.Graphics.ExtendCategories(category);
							break;
						default: throw new InvalidCastException(entityAsObj.GetType().FullName
							+ " is not castable to: LayoutEntityProto, StaticEntityProto.ID or string");
						}
					}
				}

				return null;
			}),

			#endregion

			#region edit_machine_ports

			// Append one or more new ports to an existing machine's layout. The original
			// tile footprint is preserved (sizeOverride locks CoreMin/CoreMax/LayoutSize)
			// so existing recipe port selectors and placement validation keep working —
			// only the connectivity surface changes.
			//
			// Must run before LockAndInitializeProtos: vanilla and mod machines are both
			// mutable during the ProtoRegistrator phase (mod load order: registrators run,
			// THEN protos are locked + initialized). Calling this on an already-initialized
			// proto would corrupt port-routing state — we don't guard against it because
			// the Python load path runs only during registration.
			["edit_machine_ports"] = new Constructor(["machine", "add_ports"], args => {
				// Required arg check up-front. The AnyArgument matcher
				// chain below throws a "no matching type ... received: "
				// (with an empty received-half) when Value is None,
				// which is opaque. Catching it here lets us surface the
				// real problem — the modder needs to supply a value.
				if (args["machine"]?.Value == null) {
					throw new ArgumentException(
						"edit_machine_ports: required argument `machine` is missing or None. " +
						"Pass a MachineProto, MachineProto.ID, StaticEntityProto.ID, or the id as a string.");
				}
				LayoutEntityProto target = args.GetArgument<LayoutEntityProto>("machine")
					.When<MachineProto>(m => (LayoutEntityProto)m)
					.When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
					.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<LayoutEntityProto>(id))
					.When<string>(s => registrator.PrototypesDb.GetOrThrow<LayoutEntityProto>(new Proto.ID(s)))
					.ElseRequiredThrow();

				List<Mafi.Core.Ports.Io.IoPortTemplate> extras =
					parsePortList(registrator.PrototypesDb, args, "add_ports");
				if (extras.Count == 0) return target;

				// Reject duplicate port names against the existing set — port names are
				// the recipe-side selector, so a collision would silently shadow an
				// existing port.
				var existingNames = new HashSet<char>(target.Layout.Ports.Select(p => p.Name));
				foreach (var p in extras) {
					if (!existingNames.Add(p.Name)) {
						throw new ArgumentException(
							$"edit_machine_ports[{target.Id.Value}]: port name '{p.Name}' is already used.");
					}
				}

				EntityLayout newLayout = layoutWithExtraPorts(target.Layout, extras);
				replaceLayoutOnProto(target, newLayout);
				DiagnosticTrace.Step($"edit_machine_ports[{target.Id.Value}]: " +
					$"added {extras.Count} port(s), total={newLayout.Ports.Length}");
				return target;
			}),

			#endregion

			#region build_machine

			// build_machine is a sibling of build_generator for general
			// machines. Registers a new MachineProto whose layout /
			// graphics / etc. are cloned from `source`, with name /
			// description / power / ports / research / copy_recipes and
			// the three copy_layout / copy_ports / copy_graphics opt-outs
			// overlaying the source where set.
			["build_machine"] = makeBuildMachineCtor(registrator),

			// Register a reusable custom layout tile type. Recorded into this
			// pack's token list so authored layout_str grids (machines and
			// settlement/building clones alike) can use modder-defined tiles.
			["define_box_type"] = makeDefineBoxTypeCtor(),
			["layout_token"]    = makeDefineBoxTypeCtor(),

			#endregion

			#region build_housing

			// Clone an existing SettlementHousingModuleProto under a new
			// id. Modders typically want a "variant" with different
			// capacity / unity bonus on top of an existing housing's
			// layout + graphics + needs profile. Building from scratch
			// requires unityIncreases / needsIncreases dicts the modder
			// would also need to author, so cloning is the practical
			// surface — same trade-off as build_machine vs writing a
			// MachineProto from scratch.
			["build_housing"] = makeBuildHousingCtor(registrator),

			#endregion

			#region build_settlement_decoration / build_settlement_food / build_settlement_isp / build_hospital

			// Settlement service modules — each clones from a vanilla
			// source proto and overlays a small set of typed numeric
			// overrides. Same pattern as build_housing: layout / graphics /
			// costs / needs profile come from the source verbatim; only
			// the modder-facing knobs (capacities, ranges, kW) are tunable.
			["build_settlement_decoration"] = makeBuildSettlementDecorationCtor(registrator),
			["build_settlement_food"]       = makeBuildSettlementFoodCtor(registrator),
			["build_settlement_isp"]        = makeBuildSettlementIspCtor(registrator),
			["build_hospital"]              = makeBuildHospitalCtor(registrator),

			#endregion

			#region build_mine_tower / build_research_lab / build_nuclear_reactor

			// Larger non-machine buildings that share the same clone-with-
			// overrides shape. NuclearReactor has the deepest knob surface
			// (fuel, coolant, power level, water/steam, enrichment) but
			// the structurally-heavy fields (FuelData pairs, port chars)
			// come from the source — only the numeric fuel + power knobs
			// are tunable through the Python surface.
			["build_mine_tower"]      = makeBuildMineTowerCtor(registrator),
			["build_research_lab"]    = makeBuildResearchLabCtor(registrator),
			["build_nuclear_reactor"] = makeBuildNuclearReactorCtor(registrator),
			["edit_nuclear_reactor_fuels"] = makeEditNuclearReactorFuelsCtor(registrator),
			["edit_nuclear_reactor_fluids"] = makeEditNuclearReactorFluidsCtor(registrator),
			["edit_nuclear_reactor_enrichment"] = makeEditNuclearReactorEnrichmentCtor(registrator),
			["edit_nuclear_reactor_ports"] = makeEditNuclearReactorPortsCtor(registrator),

			#endregion
		};
		// One-shot drift check. Walks the freshly-built resolver dict
		// against the ApiVersionRegistry — warns when a Constructor
		// gained an argument that wasn't recorded in the registry (the
		// most likely source of registry rot) or when the registry
		// references a call/arg that no longer exists.
		runApiVersionDriftCheckOnce(context);
		return context;
	}

	// Guarded so the check only emits warnings the first time
	// resolvers() builds a context this session. The dict is identical
	// across packs so repeating the warnings on every pack file would
	// just spam the log.
	private static bool s_driftCheckDone;
	private static void runApiVersionDriftCheckOnce(Dictionary<string, object> context) {
		if (s_driftCheckDone) return;
		s_driftCheckDone = true;
		try {
			// 1. Registry entries pointing at a missing Constructor.
			//    Most likely cause: a call was renamed in C# but the
			//    registry still references the old name.
			foreach (KeyValuePair<string, ApiCallSpec> kv in ApiVersionRegistry.Calls) {
				if (!context.TryGetValue(kv.Key, out object resolver) || !(resolver is PythonAPI.Runtime.Constructor)) {
					Log.Warning("[CustomAssets API] drift: registry has '" + kv.Key
						+ "' but no Constructor with that name is registered.");
				}
			}
			// 2. Constructor args missing from the registry. Walked
			//    per-Constructor; baseline-assumed args (no registry
			//    entry) are fine — we only flag args present on the
			//    Constructor that ALSO appear in spec.Args under a
			//    different name (typo case) or args declared as
			//    "since vX" in the registry that the Constructor no
			//    longer accepts.
			foreach (KeyValuePair<string, object> kv in context) {
				if (!(kv.Value is PythonAPI.Runtime.Constructor ctor)) continue;
				if (ctor.Arguments == null) continue;
				if (!ApiVersionRegistry.Calls.TryGetValue(kv.Key, out ApiCallSpec spec)) continue;
				HashSet<string> ctorArgs = new HashSet<string>(ctor.Arguments, StringComparer.Ordinal);
				foreach (KeyValuePair<string, ApiArgSpec> argEntry in spec.Args) {
					if (!ctorArgs.Contains(argEntry.Key)) {
						Log.Warning("[CustomAssets API] drift: registry says '"
							+ argEntry.Key + "' is an arg of " + kv.Key
							+ " (since " + argEntry.Value.Since
							+ ") but the Constructor doesn't list it.");
					}
				}
			}
		} catch (Exception ex) {
			Log.Warning("[CustomAssets API] drift check threw: " + ex.Message);
		}
	}

	// build_housing constructor body. Clones a SettlementHousingModuleProto
	// from `source` (e.g. "HousingT2") and overlays optional
	// name / description / capacity / upointsCapacity / research overrides.
	// unityIncreases + needsIncreases come from the source verbatim — these
	// are structurally complex (lists of (needs[], percent) pairs and
	// dictionaries keyed by PopNeedProto) and aren't worth a Python surface
	// when 99% of mods just want "same housing, different size + unlock".
	private static Constructor makeBuildHousingCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"housingId",       // str | StaticEntityProto.ID — required
			"source",          // str | id of existing housing — required
			"name",            // str — optional, defaults to source name
			"description",     // str — optional, defaults to source desc
			"capacity",        // int — optional, max pop count (defaults to source)
			"upointsCapacity", // int (Upoints value) — optional, unity bonus (defaults to source)
			"layout_str",      // str — optional, replaces source layout string
			// Settlement entities don't have ports in their layouts, so
			// `add_ports` is not advertised here. buildLayoutFromOverrides
			// still no-ops the check harmlessly if a Python source ever
			// passes one — see [[buildLayoutFromOverrides]].
			"research",        // research id — optional unlock
			"lockedOnInit",    // bool — optional (default: research != null)
		], args => {
			if (args["housingId"]?.Value == null) {
				throw new ArgumentException(
					"build_housing: required argument `housingId` is missing or None.");
			}
			if (args["source"]?.Value == null) {
				throw new ArgumentException(
					"build_housing: required argument `source` is missing or None.");
			}
			string newIdStr = args.GetArgument<string>("housingId")
				.When<Proto.ID>(p => p.Value)
				.When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();

			SettlementHousingModuleProto sourceProto = args.GetArgument<SettlementHousingModuleProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementHousingModuleProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementHousingModuleProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<SettlementHousingModuleProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name")
				.ElseDefault(sourceProto.Strings.Name.TranslatedString ?? sourceProto.Id.Value);
			string desc = args.GetArgument<string>("description")
				.ElseDefault(sourceProto.Strings.DescShort.TranslatedString ?? "");
			int capacity = args.GetArgument<int>("capacity")
				.ElseDefault(sourceProto.Capacity);
			int upointsCapacityRaw = args.GetArgument<int>("upointsCapacity")
				.ElseDefault(sourceProto.UpointsCapacity.Value.IntegerPart);

			DiagnosticTrace.Step($"build_housing[{newIdStr}]: source={sourceProto.Id.Value}, " +
				$"capacity={capacity}, upointsCapacity={upointsCapacityRaw}");

			EntityLayout layoutToUse = buildLayoutFromOverrides(registrator, sourceProto.Layout, args, "build_housing", newIdStr);

			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			SettlementHousingModuleProto clone = new SettlementHousingModuleProto(
				id:               newId,
				strings:          Proto.CreateStr(newId, name, desc, null),
				layout:           layoutToUse,
				costs:            sourceProto.Costs,
				capacity:         capacity,
				upointsCapacity:  new Upoints(upointsCapacityRaw),
				unityIncreases:   sourceProto.UnityIncreases,
				needsIncreases:   sourceProto.NeedsIncreases,
				graphics:         sourceProto.Graphics);

			ResearchNodeProto research = args.GetArgument<ResearchNodeProto>("research")
				.When<ResearchNodeProto.ID>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(rid))
				.When<string>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(rid)))
				.ElseNull();
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			registrator.PrototypesDb.Add(clone, locked);

			if (research != null) {
				typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
					?.SetValue(research, research.Units
						.AsEnumerable()
						.Concat(new IUnlockNodeUnit[] { new ProtoWithIconUnlock(clone, false) })
						.ToImmutableArray());
				typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
						BindingFlags.NonPublic | BindingFlags.Instance)
					?.SetValue(research.Graphics, research.Graphics.IconsProtos
						.AsEnumerable()
						.Concat(new IProtoWithIcon[] { clone })
						.Distinct()
						.ToImmutableArray());
			}

			DiagnosticTrace.Step($"build_housing[{newIdStr}]: complete (locked={locked})");
			return clone;
		});
	}

	// Shared post-construction wiring used by every build_* clone:
	//   - Add the clone to the prototypes DB with the right locked state.
	//   - When `research` is set, append the clone to the research node's
	//     Units array (so it shows as an unlock) and IconsProtos (so the
	//     research-tree icon strip displays it).
	// Centralised so the seven settlement / building clones don't each
	// re-implement the same reflection dance.
	private static void wireProtoUnlock(ProtoRegistrator registrator, IProtoWithIcon clone,
			Mafi.Core.Prototypes.Proto cloneAsProto, ResearchNodeProto research, bool locked) {
		registrator.PrototypesDb.Add(cloneAsProto, locked);
		if (research == null) return;
		typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
			?.SetValue(research, research.Units
				.AsEnumerable()
				.Concat(new IUnlockNodeUnit[] { new ProtoWithIconUnlock(clone, false) })
				.ToImmutableArray());
		typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
				BindingFlags.NonPublic | BindingFlags.Instance)
			?.SetValue(research.Graphics, research.Graphics.IconsProtos
				.AsEnumerable()
				.Concat(new IProtoWithIcon[] { clone })
				.Distinct()
				.ToImmutableArray());
	}

	// Resolve the `research` arg (string / ID / typed proto) → ResearchNodeProto or null.
	private static ResearchNodeProto resolveResearch(ProtoRegistrator registrator,
			Constructor.CallArguments args) {
		return args.GetArgument<ResearchNodeProto>("research")
			.When<ResearchNodeProto.ID>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(rid))
			.When<string>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(rid)))
			.ElseNull();
	}

	// Read a private Fix32 field on a proto via reflection. Used to read
	// the "consumed-per-pop" coefficients on Hospital / ISP modules
	// (Fix32 fields exposed only through GetX methods on the proto, so
	// the only way to fetch the literal value for a clone is via the
	// backing field). NonPublic | Instance covers both `private readonly`
	// fields and auto-property backing fields.
	private static Fix32 readPrivateFix32(object obj, string fieldName) {
		FieldInfo f = obj.GetType().GetField(fieldName,
			BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
		if (f == null) {
			throw new InvalidOperationException(
				$"readPrivateFix32: field '{fieldName}' not found on {obj.GetType().FullName}.");
		}
		return (Fix32)f.GetValue(obj);
	}

	// Common required-id check used by every build_* clone — emits a
	// readable "missing or None" error instead of the opaque AnyArgument
	// chain trailer when the editor saves a draft def with an empty id.
	private static void requireArg(Constructor.CallArguments args, string fnName, string argName) {
		if (args[argName]?.Value == null) {
			throw new ArgumentException(
				fnName + ": required argument `" + argName + "` is missing or None.");
		}
	}

	// build_settlement_decoration — clones SettlementDecorationModuleProto.
	// Tunable: id, name, description, upointsBonusToNearbyHousing, bonusRange.
	private static Constructor makeBuildSettlementDecorationCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"decorationId", "source", "name", "description",
			"upointsBonus", "bonusRange",
			"layout_str",
			"research", "lockedOnInit",
		], args => {
			requireArg(args, "build_settlement_decoration", "decorationId");
			requireArg(args, "build_settlement_decoration", "source");
			string newIdStr = args.GetArgument<string>("decorationId")
				.When<Proto.ID>(p => p.Value).When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();
			SettlementDecorationModuleProto src = args.GetArgument<SettlementDecorationModuleProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementDecorationModuleProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementDecorationModuleProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<SettlementDecorationModuleProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name").ElseDefault(src.Strings.Name.TranslatedString ?? src.Id.Value);
			string desc = args.GetArgument<string>("description").ElseDefault(src.Strings.DescShort.TranslatedString ?? "");
			int upointsBonus = args.GetArgument<int>("upointsBonus").ElseDefault(src.UpointsBonusToNearbyHousing.Value.IntegerPart);
			int bonusRange   = args.GetArgument<int>("bonusRange").ElseDefault(src.BonusRange);

			EntityLayout layoutToUse = buildLayoutFromOverrides(registrator, src.Layout, args, "build_settlement_decoration", newIdStr);
			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			SettlementDecorationModuleProto clone = new SettlementDecorationModuleProto(
				id:                          newId,
				strings:                     Proto.CreateStr(newId, name, desc, null),
				layout:                      layoutToUse,
				costs:                       src.Costs,
				upointsBonusToNearbyHousing: new Upoints(upointsBonus),
				bonusRange:                  bonusRange,
				graphics:                    src.Graphics);
			ResearchNodeProto research = resolveResearch(registrator, args);
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			wireProtoUnlock(registrator, clone, clone, research, locked);
			return clone;
		});
	}

	// build_settlement_food — clones SettlementFoodModuleProto.
	// Tunable: id, name, description, buffersCount, capacityPerBuffer.
	private static Constructor makeBuildSettlementFoodCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"foodModuleId", "source", "name", "description",
			"buffersCount", "capacityPerBuffer",
			"layout_str",
			"research", "lockedOnInit",
		], args => {
			requireArg(args, "build_settlement_food", "foodModuleId");
			requireArg(args, "build_settlement_food", "source");
			string newIdStr = args.GetArgument<string>("foodModuleId")
				.When<Proto.ID>(p => p.Value).When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();
			SettlementFoodModuleProto src = args.GetArgument<SettlementFoodModuleProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementFoodModuleProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementFoodModuleProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<SettlementFoodModuleProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name").ElseDefault(src.Strings.Name.TranslatedString ?? src.Id.Value);
			string desc = args.GetArgument<string>("description").ElseDefault(src.Strings.DescShort.TranslatedString ?? "");
			int buffersCount      = args.GetArgument<int>("buffersCount").ElseDefault(src.BuffersCount);
			int capacityPerBuffer = args.GetArgument<int>("capacityPerBuffer").ElseDefault(src.CapacityPerBuffer.Value);

			EntityLayout layoutToUse = buildLayoutFromOverrides(registrator, src.Layout, args, "build_settlement_food", newIdStr);
			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			SettlementFoodModuleProto clone = new SettlementFoodModuleProto(
				id:                newId,
				strings:           Proto.CreateStr(newId, name, desc, null),
				layout:            layoutToUse,
				costs:             src.Costs,
				buffersCount:      buffersCount,
				capacityPerBuffer: new Quantity(capacityPerBuffer),
				graphics:          src.Graphics);
			ResearchNodeProto research = resolveResearch(registrator, args);
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			wireProtoUnlock(registrator, clone, clone, research, locked);
			return clone;
		});
	}

	// build_settlement_isp — clones SettlementIspModuleProto.
	// Tunable: id, name, description, computingPer100Pops, electricityConsumedKw.
	private static Constructor makeBuildSettlementIspCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"ispModuleId", "source", "name", "description",
			"computingPer100Pops", "electricityConsumedKw",
			"layout_str",
			"research", "lockedOnInit",
		], args => {
			requireArg(args, "build_settlement_isp", "ispModuleId");
			requireArg(args, "build_settlement_isp", "source");
			string newIdStr = args.GetArgument<string>("ispModuleId")
				.When<Proto.ID>(p => p.Value).When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();
			SettlementIspModuleProto src = args.GetArgument<SettlementIspModuleProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementIspModuleProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<SettlementIspModuleProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<SettlementIspModuleProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name").ElseDefault(src.Strings.Name.TranslatedString ?? src.Id.Value);
			string desc = args.GetArgument<string>("description").ElseDefault(src.Strings.DescShort.TranslatedString ?? "");
			// The computingPer100Pops field on SettlementIspModuleProto
			// is private (m_computingPer100Pops); read it via reflection
			// so the default-from-source still works without surfacing
			// a Fix32 type to Python.
			int sourceComputingPer100 = readPrivateFix32(src,
				"m_computingPer100Pops").IntegerPart;
			int computingPer100PopsRaw = args.GetArgument<int>("computingPer100Pops")
				.ElseDefault(sourceComputingPer100);
			int electricityKw = args.GetArgument<int>("electricityConsumedKw")
				.ElseDefault((int)src.ElectricityConsumed.Value);

			EntityLayout layoutToUse = buildLayoutFromOverrides(registrator, src.Layout, args, "build_settlement_isp", newIdStr);
			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			SettlementIspModuleProto clone = new SettlementIspModuleProto(
				id:                  newId,
				strings:             Proto.CreateStr(newId, name, desc, null),
				layout:              layoutToUse,
				costs:               src.Costs,
				need:                src.PopsNeed,
				computingPer100Pops: Fix32.FromInt(computingPer100PopsRaw),
				electricityConsumed: Mafi.Electricity.FromKw(electricityKw),
				emissionIntensity:   src.EmissionIntensity,
				graphics:            src.Graphics);
			ResearchNodeProto research = resolveResearch(registrator, args);
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			wireProtoUnlock(registrator, clone, clone, research, locked);
			return clone;
		});
	}

	// build_hospital — clones HospitalProto.
	// Tunable: id, name, description, powerRequiredKw, buffersCount,
	// capacityPerBuffer, suppliesPerHundredPopsPerMonth.
	private static Constructor makeBuildHospitalCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"hospitalId", "source", "name", "description",
			"powerRequiredKw", "buffersCount", "capacityPerBuffer",
			"suppliesPerHundredPopsPerMonth",
			"layout_str",
			"research", "lockedOnInit",
		], args => {
			requireArg(args, "build_hospital", "hospitalId");
			requireArg(args, "build_hospital", "source");
			string newIdStr = args.GetArgument<string>("hospitalId")
				.When<Proto.ID>(p => p.Value).When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();
			HospitalProto src = args.GetArgument<HospitalProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<HospitalProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<HospitalProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<HospitalProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name").ElseDefault(src.Strings.Name.TranslatedString ?? src.Id.Value);
			string desc = args.GetArgument<string>("description").ElseDefault(src.Strings.DescShort.TranslatedString ?? "");
			int powerKw = args.GetArgument<int>("powerRequiredKw").ElseDefault((int)src.PowerRequired.Value);
			int buffersCount = args.GetArgument<int>("buffersCount").ElseDefault(src.BuffersCount);
			int capPerBuffer = args.GetArgument<int>("capacityPerBuffer").ElseDefault(src.CapacityPerBuffer.Value);
			// SuppliesConsumedPerHundredPopsPerMonth on HospitalProto is
			// private — read it via reflection same as the ISP variant.
			int sourceSuppliesPer100 = readPrivateFix32(src,
				"SuppliesConsumedPerHundredPopsPerMonth").IntegerPart;
			int suppliesPer100 = args.GetArgument<int>("suppliesPerHundredPopsPerMonth")
				.ElseDefault(sourceSuppliesPer100);

			EntityLayout layoutToUse = buildLayoutFromOverrides(registrator, src.Layout, args, "build_hospital", newIdStr);
			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			HospitalProto clone = new HospitalProto(
				id:                                       newId,
				strings:                                  Proto.CreateStr(newId, name, desc, null),
				layout:                                   layoutToUse,
				costs:                                    src.Costs,
				need:                                     src.PopsNeed,
				powerRequired:                            Mafi.Electricity.FromKw(powerKw),
				buffersCount:                             buffersCount,
				capacityPerBuffer:                        new Quantity(capPerBuffer),
				suppliesConsumedPerHundredPopsPerMonth:   Fix32.FromInt(suppliesPer100),
				animationParams:                          src.AnimationParams,
				emissionIntensity:                        src.EmissionIntensity,
				graphics:                                 src.Graphics);
			ResearchNodeProto research = resolveResearch(registrator, args);
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			wireProtoUnlock(registrator, clone, clone, research, locked);
			return clone;
		});
	}

	// build_mine_tower — clones MineTowerProto. Tunable: id, name,
	// description. The MineArea (origin + size + max edge) comes from
	// source — modders rarely want a custom mining footprint, and the
	// structurally-complex RelTile2i origin/size pair isn't worth a
	// Python surface for now.
	private static Constructor makeBuildMineTowerCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"mineTowerId", "source", "name", "description",
			"layout_str",
			"research", "lockedOnInit",
		], args => {
			requireArg(args, "build_mine_tower", "mineTowerId");
			requireArg(args, "build_mine_tower", "source");
			string newIdStr = args.GetArgument<string>("mineTowerId")
				.When<Proto.ID>(p => p.Value).When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();
			MineTowerProto src = args.GetArgument<MineTowerProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MineTowerProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<MineTowerProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<MineTowerProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name").ElseDefault(src.Strings.Name.TranslatedString ?? src.Id.Value);
			string desc = args.GetArgument<string>("description").ElseDefault(src.Strings.DescShort.TranslatedString ?? "");

			EntityLayout layoutToUse = buildLayoutFromOverrides(registrator, src.Layout, args, "build_mine_tower", newIdStr);
			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			MineTowerProto clone = new MineTowerProto(
				id:       newId,
				strings:  Proto.CreateStr(newId, name, desc, null),
				layout:   layoutToUse,
				costs:    src.Costs,
				area:     src.Area,
				graphics: src.Graphics);
			ResearchNodeProto research = resolveResearch(registrator, args);
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			wireProtoUnlock(registrator, clone, clone, research, locked);
			return clone;
		});
	}

	// build_research_lab — clones ResearchLabProto. Tunable knobs cover
	// the typical "faster lab + more science per recipe" mod surface;
	// computing, recipe products + tier index inherit from source.
	private static Constructor makeBuildResearchLabCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"researchLabId", "source", "name", "description",
			"electricityConsumedKw", "computingConsumed",
			"durationForRecipeSeconds", "sciencePerRecipe",
			"unityMonthlyCost",
			"add_ports", "layout_str",
			"research", "lockedOnInit",
		], args => {
			requireArg(args, "build_research_lab", "researchLabId");
			requireArg(args, "build_research_lab", "source");
			string newIdStr = args.GetArgument<string>("researchLabId")
				.When<Proto.ID>(p => p.Value).When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();
			ResearchLabProto src = args.GetArgument<ResearchLabProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchLabProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchLabProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<ResearchLabProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name").ElseDefault(src.Strings.Name.TranslatedString ?? src.Id.Value);
			string desc = args.GetArgument<string>("description").ElseDefault(src.Strings.DescShort.TranslatedString ?? "");
			int kw = args.GetArgument<int>("electricityConsumedKw").ElseDefault((int)src.ElectricityConsumed.Value);
			int computing = args.GetArgument<int>("computingConsumed").ElseDefault((int)src.ComputingConsumed.Value);
			int durationSec = args.GetArgument<int>("durationForRecipeSeconds").ElseDefault(src.DurationOfRecipe.SecondsFloored);
			int sciencePerRecipe = args.GetArgument<int>("sciencePerRecipe").ElseDefault(src.SciencePerRecipe.IntegerPart);
			int unityCost = args.GetArgument<int>("unityMonthlyCost").ElseDefault(src.UnityMonthlyCost.Value.IntegerPart);

			EntityLayout layoutToUse = buildLayoutFromOverrides(registrator, src.Layout, args, "build_research_lab", newIdStr);
			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			ResearchLabProto clone = new ResearchLabProto(
				id:                  newId,
				strings:             Proto.CreateStr(newId, name, desc, null),
				layout:              layoutToUse,
				costs:               src.Costs,
				electricityConsumed: Mafi.Electricity.FromKw(kw),
				computingConsumed:   new Mafi.Computing(computing),
				unityMonthlyCost:    new Upoints(unityCost),
				upointsCategory:     src.UpointsCategory,
				durationForRecipe:   Duration.FromSec(durationSec),
				sciencePerRecipe:    Fix32.FromInt(sciencePerRecipe),
				consumedPerRecipe:   src.ConsumedPerRecipe,
				producedPerRecipe:   src.ProducedPerRecipe,
				inputBufferCapacity:  src.InputBufferCapacity,
				outputBufferCapacity: src.OutputBufferCapacity,
				animationParams:     src.AnimationParams,
				tierIndex:           src.TierIndex,
				graphics:            src.Graphics,
				emissionIntensity:   src.EmissionIntensity);
			ResearchNodeProto research = resolveResearch(registrator, args);
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			wireProtoUnlock(registrator, clone, clone, research, locked);
			return clone;
		});
	}

	// build_nuclear_reactor — clones NuclearReactorProto. Tunable: id /
	// name / description / power level / fuel & coolant capacities /
	// process duration / kW. Structurally heavy fields (FuelData pairs,
	// port chars, enrichment data, ProductProto refs for coolant) come
	// from the source — modders shop for "faster / bigger / different
	// research gate" variants of an existing reactor, not custom fuel
	// chemistries.
	// edit_nuclear_reactor_fuels(reactor, add_fuels). Resolves the target
	// NuclearReactorProto, parses the FuelPair list into runtime FuelData
	// entries, and APPENDS them to the proto's FuelPairs field + Recipes
	// list via reflection.
	//
	// Why reflection: NuclearReactorProto's FuelPairs is `public readonly
	// ImmutableArray<FuelData>` — set once in the ctor and never mutated.
	// Recipes is a `public IIndexable<IRecipeForUi> Recipes { get; }`
	// auto-property whose backing field references the Lyst populated by
	// the ctor. To extend either without re-building the whole proto we
	// reflect on the backing fields. This is safe ONLY before
	// LockAndInitializeProtos runs — the Python load path always runs at
	// the right time, so the editor doesn't expose any way to hit this
	// after game start.
	private static Constructor makeEditNuclearReactorFuelsCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"reactor",
			"add_fuels",
		], args => {
			if (args["reactor"]?.Value == null) {
				throw new ArgumentException(
					"edit_nuclear_reactor_fuels: required argument `reactor` is missing or None. " +
					"Pass the reactor id (string), MachineProto.ID, or NuclearReactorProto instance.");
			}
			NuclearReactorProto target = args.GetArgument<NuclearReactorProto>("reactor")
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			// Parse new fuels — each FuelPair Python struct → runtime FuelData(fuelIn, spentOut, duration).
			var newFuels = new Lyst<NuclearReactorProto.FuelData>();
			if (args.GetArgument<List<object>>("add_fuels").WhenExists(out var rawFuels)) {
				foreach (object item in rawFuels) {
					if (!(item is FuelPair fp)) {
						throw new ArgumentException(
							"edit_nuclear_reactor_fuels: add_fuels list item must be FuelPair, got " +
							(item?.GetType()?.FullName ?? "null") + ".");
					}
					newFuels.Add(new NuclearReactorProto.FuelData(
						fp.fuelIn, fp.spentFuelOut, Duration.FromSec(fp.durationSeconds)));
				}
			}
			if (newFuels.Count == 0) {
				Log.Warning($"edit_nuclear_reactor_fuels[{target.Id.Value}]: add_fuels is empty — no-op.");
				return target;
			}

			// Build the combined FuelPairs array (existing + new) and
			// reflect-write the FuelPairs field. ImmutableArray is a
			// readonly value-type so we must replace the whole field.
			var combinedLyst = new Lyst<NuclearReactorProto.FuelData>();
			foreach (var fp in target.FuelPairs) combinedLyst.Add(fp);
			foreach (var fp in newFuels) combinedLyst.Add(fp);
			var combined = combinedLyst.ToImmutableArray();

			FieldInfo fuelPairsField = typeof(NuclearReactorProto).GetField(
				"FuelPairs", BindingFlags.Public | BindingFlags.Instance);
			if (fuelPairsField == null) {
				throw new InvalidOperationException(
					"edit_nuclear_reactor_fuels: FuelPairs field not found on NuclearReactorProto " +
					"(COI internals changed — file an issue).");
			}
			fuelPairsField.SetValue(target, combined);

			// Recipes is a Lyst<IRecipeForUi> set during the ctor —
			// we Add NuclearReactor.Recipe entries for each new fuel.
			// The backing field for `public IIndexable<IRecipeForUi> Recipes { get; }`
			// is `<Recipes>k__BackingField`. The runtime instance is a
			// Lyst — cast and Add.
			FieldInfo recipesField = typeof(NuclearReactorProto).GetField(
				"<Recipes>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
			object recipesValue = recipesField?.GetValue(target);
			if (recipesValue is Lyst<Mafi.Core.Factory.Recipes.IRecipeForUi> recipesLyst) {
				foreach (var fp in newFuels) {
					var recipe = new NuclearReactor.Recipe(
						target, fp,
						target.MaxPowerLevel * Percent.Hundred,
						1, Percent.Hundred);
					recipesLyst.Add(recipe);
				}
			} else {
				Log.Warning($"edit_nuclear_reactor_fuels[{target.Id.Value}]: Recipes backing field " +
					"is not a Lyst<IRecipeForUi> — recipe list not updated. The fuel pairs were " +
					"appended; the in-game recipe UI may be stale until next mod reload.");
			}

			DiagnosticTrace.Step($"edit_nuclear_reactor_fuels[{target.Id.Value}]: " +
				$"appended {newFuels.Count} fuel pair(s); total {combined.Length}.");
			return target;
		});
	}

	// edit_nuclear_reactor_fluids(reactor, coolantIn?, coolantOut?,
	//     coolantInPort?, coolantOutPort?, coolantInPortShape?,
	//     coolantOutPortShape?, waterInProduct?, waterInQuantity?,
	//     steamOutProduct?, steamOutQuantity?, waterInPorts?,
	//     steamOutPorts?). Patches the coolant / water / steam fields
	// on an existing NuclearReactorProto in place via reflection — same
	// "set the public readonly field" pattern that edit_nuclear_reactor_fuels
	// uses. Every arg defaults to "inherit target's existing value", so a
	// modder can override just the one piece they need.
	private static Constructor makeEditNuclearReactorFluidsCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"reactor",
			"coolantIn", "coolantOut", "coolantInPort", "coolantOutPort",
			"coolantInPortShape", "coolantOutPortShape",
			"waterInProduct", "waterInQuantity",
			"steamOutProduct", "steamOutQuantity",
			"waterInPorts", "steamOutPorts",
		], args => {
			if (args["reactor"]?.Value == null) {
				throw new ArgumentException(
					"edit_nuclear_reactor_fluids: required argument `reactor` is missing or None.");
			}
			NuclearReactorProto target = args.GetArgument<NuclearReactorProto>("reactor")
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			int changes = 0;

			Mafi.Core.Products.ProductProto coolantInOverride = args.GetArgument<Mafi.Core.Products.ProductProto>("coolantIn")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseNull();
			if (coolantInOverride != null) { setReadonlyField(target, "CoolantIn", coolantInOverride); changes++; }

			Mafi.Core.Products.ProductProto coolantOutOverride = args.GetArgument<Mafi.Core.Products.ProductProto>("coolantOut")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseNull();
			if (coolantOutOverride != null) { setReadonlyField(target, "CoolantOut", coolantOutOverride); changes++; }

			if (args.GetArgument<string>("coolantInPort").WhenExists(out string ciPort) && ciPort.Length > 0) {
				setReadonlyField(target, "CoolantInPort", ciPort[0]); changes++;
			}
			if (args.GetArgument<string>("coolantOutPort").WhenExists(out string coPort) && coPort.Length > 0) {
				setReadonlyField(target, "CoolantOutPort", coPort[0]); changes++;
			}

			// Port-shape overrides patch the Layout's port array (via the
			// same substitution helper build_nuclear_reactor uses for the
			// fuel ports) and then replace the proto's Layout/InputPorts/
			// OutputPorts. Done last so coolantInPort / coolantOutPort
			// overrides above have already taken effect when we look up
			// the port name to substitute.
			var portShapeChanges = new Dictionary<char, Mafi.Core.Ports.Io.IoPortShapeProto>();
			if (args.GetArgument<string>("coolantInPortShape").WhenExists(out string ciShapeId) && !string.IsNullOrEmpty(ciShapeId)) {
				portShapeChanges[target.CoolantInPort] = resolvePortShape(registrator.PrototypesDb, ciShapeId);
			}
			if (args.GetArgument<string>("coolantOutPortShape").WhenExists(out string coShapeId) && !string.IsNullOrEmpty(coShapeId)) {
				portShapeChanges[target.CoolantOutPort] = resolvePortShape(registrator.PrototypesDb, coShapeId);
			}
			if (portShapeChanges.Count > 0) {
				EntityLayout newLayout = layoutWithSubstitutedPortShapes(target.Layout, portShapeChanges);
				replaceLayoutOnProto(target, newLayout);
				changes++;
			}

			// Water-in / steam-out ProductQuantity overrides.
			Mafi.Core.Products.ProductProto waterProto = args.GetArgument<Mafi.Core.Products.ProductProto>("waterInProduct")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseNull();
			int? waterQty = args.GetArgument<int>("waterInQuantity").WhenExists(out int wq) ? (int?)wq : null;
			if (waterProto != null || waterQty.HasValue) {
				Mafi.Core.ProductQuantity newWaterPq = new Mafi.Core.ProductQuantity(
					waterProto ?? target.WaterInPerPowerLevel.Product,
					new Quantity(waterQty ?? target.WaterInPerPowerLevel.Quantity.Value));
				setReadonlyField(target, "WaterInPerPowerLevel", newWaterPq);
				changes++;
			}

			Mafi.Core.Products.ProductProto steamProto = args.GetArgument<Mafi.Core.Products.ProductProto>("steamOutProduct")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseNull();
			int? steamQty = args.GetArgument<int>("steamOutQuantity").WhenExists(out int sq) ? (int?)sq : null;
			if (steamProto != null || steamQty.HasValue) {
				Mafi.Core.ProductQuantity newSteamPq = new Mafi.Core.ProductQuantity(
					steamProto ?? target.SteamOutPerPowerLevel.Product,
					new Quantity(steamQty ?? target.SteamOutPerPowerLevel.Quantity.Value));
				setReadonlyField(target, "SteamOutPerPowerLevel", newSteamPq);
				changes++;
			}

			if (args.GetArgument<string>("waterInPorts").WhenExists(out string waterPorts) && !string.IsNullOrEmpty(waterPorts)) {
				setReadonlyField(target, "WaterInPorts", waterPorts); changes++;
			}
			if (args.GetArgument<string>("steamOutPorts").WhenExists(out string steamPorts) && !string.IsNullOrEmpty(steamPorts)) {
				setReadonlyField(target, "SteamOutPorts", steamPorts); changes++;
			}

			if (changes == 0) {
				Log.Warning($"edit_nuclear_reactor_fluids[{target.Id.Value}]: no override fields supplied — no-op.");
			} else {
				DiagnosticTrace.Step($"edit_nuclear_reactor_fluids[{target.Id.Value}]: applied {changes} change(s).");
			}
			return target;
		});
	}

	// edit_nuclear_reactor_enrichment(reactor, enrichment=Enrichment(...))
	// Replaces the target reactor's Enrichment field with the supplied
	// override (resolved through buildEnrichmentDataFromPython so partial
	// overrides inherit any missing fields from the target's existing
	// Enrichment, just like build_nuclear_reactor does against the source).
	private static Constructor makeEditNuclearReactorEnrichmentCtor(ProtoRegistrator registrator) {
		return new Constructor(["reactor", "enrichment"], args => {
			if (args["reactor"]?.Value == null) {
				throw new ArgumentException(
					"edit_nuclear_reactor_enrichment: required argument `reactor` is missing or None.");
			}
			NuclearReactorProto target = args.GetArgument<NuclearReactorProto>("reactor")
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(new Proto.ID(s)))
				.ElseRequiredThrow();
			if (!(args["enrichment"]?.Value is Enrichment py)) {
				throw new ArgumentException(
					"edit_nuclear_reactor_enrichment: required argument `enrichment` must be an Enrichment(...) value.");
			}
			Mafi.Option<NuclearReactorProto.EnrichmentData> merged = buildEnrichmentDataFromPython(py, target.Enrichment);
			setReadonlyField(target, "Enrichment", merged);
			DiagnosticTrace.Step($"edit_nuclear_reactor_enrichment[{target.Id.Value}]: enrichment patched " +
				$"({(merged.HasValue ? "set" : "cleared")}).");
			return target;
		});
	}

	// edit_nuclear_reactor_ports(reactor, add_ports=[Port(...), ...]) —
	// reactor-typed sibling of edit_machine_ports. Useful for adding the
	// enrichment in/out ports (or any auxiliary connection) to an
	// existing NuclearReactorProto without cloning it. Same constraints:
	// must run during the registration phase, and new port names must
	// not collide with the reactor's existing fuel / water / steam /
	// coolant ports.
	private static Constructor makeEditNuclearReactorPortsCtor(ProtoRegistrator registrator) {
		return new Constructor(["reactor", "add_ports"], args => {
			if (args["reactor"]?.Value == null) {
				throw new ArgumentException(
					"edit_nuclear_reactor_ports: required argument `reactor` is missing or None. " +
					"Pass a NuclearReactorProto, its id, or the id as a string.");
			}
			NuclearReactorProto target = args.GetArgument<NuclearReactorProto>("reactor")
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			List<Mafi.Core.Ports.Io.IoPortTemplate> extras =
				parsePortListStatic(registrator.PrototypesDb, args, "add_ports");
			if (extras.Count == 0) {
				Log.Warning($"edit_nuclear_reactor_ports[{target.Id.Value}]: add_ports was empty — no-op.");
				return target;
			}

			var existingNames = new HashSet<char>(target.Layout.Ports.Select(p => p.Name));
			foreach (var p in extras) {
				if (!existingNames.Add(p.Name)) {
					throw new ArgumentException(
						$"edit_nuclear_reactor_ports[{target.Id.Value}]: port name '{p.Name}' is already used.");
				}
			}

			EntityLayout newLayout = layoutWithExtraPorts(target.Layout, extras);
			replaceLayoutOnProto(target, newLayout);
			DiagnosticTrace.Step($"edit_nuclear_reactor_ports[{target.Id.Value}]: " +
				$"added {extras.Count} port(s), total={newLayout.Ports.Length}");
			return target;
		});
	}

	// Reflect-write any public readonly field on a NuclearReactorProto.
	// Used by edit_nuclear_reactor_fluids / edit_nuclear_reactor_enrichment
	// to patch the proto in place. Throws when the field name doesn't
	// resolve so a COI internals rename surfaces with a clear error
	// instead of silently no-op'ing.
	private static void setReadonlyField(object target, string fieldName, object value) {
		FieldInfo field = typeof(NuclearReactorProto).GetField(
			fieldName, BindingFlags.Public | BindingFlags.Instance);
		if (field == null) {
			throw new InvalidOperationException(
				$"NuclearReactorProto.{fieldName} field not found — COI internals changed.");
		}
		field.SetValue(target, value);
	}

	private static Constructor makeBuildNuclearReactorCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"reactorId", "source", "name", "description",
			"maxPowerLevel", "fuelCapacity", "minFuelToOperate",
			"processDurationSeconds", "computingConsumed",
			"fuel_pairs",
			"fuelInPortShape", "fuelOutPortShape",
			"coolantIn", "coolantOut", "coolantInPort", "coolantOutPort",
			"coolantInPortShape", "coolantOutPortShape",
			"waterInProduct", "waterInQuantity",
			"steamOutProduct", "steamOutQuantity",
			"waterInPorts", "steamOutPorts",
			"enrichment",
			"add_ports", "layout_str",
			"research", "lockedOnInit",
		], args => {
			requireArg(args, "build_nuclear_reactor", "reactorId");
			requireArg(args, "build_nuclear_reactor", "source");
			string newIdStr = args.GetArgument<string>("reactorId")
				.When<Proto.ID>(p => p.Value).When<StaticEntityProto.ID>(p => p.Value)
				.ElseRequiredThrow();
			NuclearReactorProto src = args.GetArgument<NuclearReactorProto>("source")
				.When<StaticEntityProto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<Proto.ID>(id => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<NuclearReactorProto>(new Proto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name").ElseDefault(src.Strings.Name.TranslatedString ?? src.Id.Value);
			string desc = args.GetArgument<string>("description").ElseDefault(src.Strings.DescShort.TranslatedString ?? "");
			int maxPowerLevel = args.GetArgument<int>("maxPowerLevel").ElseDefault(src.MaxPowerLevel);
			int fuelCapacity = args.GetArgument<int>("fuelCapacity").ElseDefault(src.FuelCapacity.Value);
			int minFuel = args.GetArgument<int>("minFuelToOperate").ElseDefault(src.MinFuelToOperate.Value);
			int durationSec = args.GetArgument<int>("processDurationSeconds").ElseDefault(src.ProcessDuration.SecondsFloored);
			int computing = args.GetArgument<int>("computingConsumed").ElseDefault((int)src.ComputingConsumed.Value);

			// Optional fuel_pairs override. When the modder supplies a
			// non-empty list, we REPLACE the source reactor's FuelPairs
			// array; otherwise the source's chemistry list comes through
			// unchanged. Each Python-side FuelPair is converted to the
			// runtime NuclearReactorProto.FuelData(fuelIn, spentOut,
			// duration) struct. Empty/missing list falls through to source.
			var sourceFuelPairs = src.FuelPairs;
			bool fuelPairsOverridden = false;
			Mafi.Core.Products.ProductProto firstFuelInProto = null;
			Mafi.Core.Products.ProductProto firstSpentFuelOutProto = null;
			if (args.GetArgument<List<object>>("fuel_pairs").WhenExists(out var rawFuelPairs)
					&& rawFuelPairs.Count > 0) {
				var pairsLyst = new Lyst<NuclearReactorProto.FuelData>();
				foreach (object item in rawFuelPairs) {
					if (!(item is FuelPair fp)) {
						throw new ArgumentException($"build_nuclear_reactor: fuel_pairs item must be FuelPair, got " +
							(item?.GetType()?.FullName ?? "null") + ".");
					}
					pairsLyst.Add(new NuclearReactorProto.FuelData(
						fp.fuelIn, fp.spentFuelOut, Duration.FromSec(fp.durationSeconds)));
					if (firstFuelInProto == null) firstFuelInProto = fp.fuelIn;
					if (firstSpentFuelOutProto == null) firstSpentFuelOutProto = fp.spentFuelOut;
				}
				sourceFuelPairs = pairsLyst.ToImmutableArray();
				fuelPairsOverridden = true;
				DiagnosticTrace.Step($"build_nuclear_reactor[{newIdStr}]: fuel_pairs overridden ({rawFuelPairs.Count} entries)");
			}

			// Optional explicit port-shape overrides. Take precedence over
			// auto-detection from the first fuel pair's product type, so
			// the modder can mix fuel chemistries with different product
			// shapes (e.g. unit-typed rods + loose-typed coal in the same
			// reactor) by picking the port shape directly.
			//
			// Validate the override ids against the live PrototypesDb up
			// front (instead of waiting for the layout substitution to
			// fail) — gives the modder a readable "shape id 'X' isn't
			// registered" error pointing at the bad arg instead of a
			// downstream layout-parse exception.
			string fuelInShapeIdOverride  = args.GetArgument<string>("fuelInPortShape").ElseDefault(null);
			string fuelOutShapeIdOverride = args.GetArgument<string>("fuelOutPortShape").ElseDefault(null);
			if (!string.IsNullOrEmpty(fuelInShapeIdOverride)
					&& !registrator.PrototypesDb.TryGetProto<Mafi.Core.Ports.Io.IoPortShapeProto>(
						new Mafi.Core.Ports.Io.IoPortShapeProto.ID(fuelInShapeIdOverride), out _)) {
				throw new ArgumentException(
					$"build_nuclear_reactor[{newIdStr}]: fuelInPortShape '{fuelInShapeIdOverride}' " +
					"is not a registered IoPortShapeProto id.");
			}
			if (!string.IsNullOrEmpty(fuelOutShapeIdOverride)
					&& !registrator.PrototypesDb.TryGetProto<Mafi.Core.Ports.Io.IoPortShapeProto>(
						new Mafi.Core.Ports.Io.IoPortShapeProto.ID(fuelOutShapeIdOverride), out _)) {
				throw new ArgumentException(
					$"build_nuclear_reactor[{newIdStr}]: fuelOutPortShape '{fuelOutShapeIdOverride}' " +
					"is not a registered IoPortShapeProto id.");
			}

			// Port substitution. The source reactor's fuel-in / fuel-out
			// ports carry a shape tuned for the vanilla fuel chemistry
			// (e.g. uranium-rod conveyor). If the modder swapped to a
			// different product type (coal/loose, oil/fluid, ...), the
			// source's port shapes won't accept the new product in-game
			// and the reactor LOOKS like the source but can't be fed —
			// the "replaced the old one" symptom. Substitute the layout
			// chars for ONLY the named fuel ports (src.FuelInPort /
			// src.FuelOutPort), so water / steam / coolant ports stay
			// untouched. Trigger when EITHER an explicit shape override
			// was provided OR fuel_pairs was overridden.
			EntityLayout reactorLayout = src.Layout;
			bool needsSubstitution = fuelPairsOverridden
				|| !string.IsNullOrEmpty(fuelInShapeIdOverride)
				|| !string.IsNullOrEmpty(fuelOutShapeIdOverride);

			// Resolve the desired in / out shapes. Explicit override
			// wins; otherwise infer from the first fuel pair's
			// product types; otherwise (no override + no fuel
			// override) keep the source's shape unchanged.
			Mafi.Core.Ports.Io.IoPortShapeProto inputShape  =
				!string.IsNullOrEmpty(fuelInShapeIdOverride)
					? resolvePortShape(registrator.PrototypesDb, fuelInShapeIdOverride)
					: (firstFuelInProto != null
						? portShapeForProduct(registrator.PrototypesDb, firstFuelInProto)
						: null);
			Mafi.Core.Ports.Io.IoPortShapeProto outputShape =
				!string.IsNullOrEmpty(fuelOutShapeIdOverride)
					? resolvePortShape(registrator.PrototypesDb, fuelOutShapeIdOverride)
					: (firstSpentFuelOutProto != null
						? portShapeForProduct(registrator.PrototypesDb, firstSpentFuelOutProto)
						: null);

			// Per-name shape substitution. The previous global string.Replace
			// approach couldn't disambiguate when the source's fuel-in and
			// fuel-out ports shared the SAME shape char (the common case:
			// both 'F' and 'S' on the vanilla NuclearReactor use
			// IoPortShape_FlatConveyor layout char '#'). Per-port-name
			// substitution rebuilds the IoPortTemplate by lookup on
			// Spec.Name, so two ports with identical original shape but
			// different desired new shapes both land correctly.
			var portNameToNewShape = new Dictionary<char, Mafi.Core.Ports.Io.IoPortShapeProto>();
			if (inputShape != null && src.FuelInPort != default(char)) {
				portNameToNewShape[src.FuelInPort] = inputShape;
			}
			if (outputShape != null && src.FuelOutPort != default(char)) {
				portNameToNewShape[src.FuelOutPort] = outputShape;
			}
			foreach (var p in src.Layout.Ports) {
				if (portNameToNewShape.TryGetValue(p.Spec.Name, out var newShape)) {
					DiagnosticTrace.Step($"build_nuclear_reactor[{newIdStr}]: port '{p.Spec.Name}' " +
						$"shape '{p.Spec.Shape.Id.Value}' (char '{p.Spec.Shape.LayoutChar}') " +
						$"-> '{newShape.Id.Value}' (char '{newShape.LayoutChar}')");
				}
			}
			if (portNameToNewShape.Count > 0) {
				reactorLayout = layoutWithSubstitutedPortShapes(src.Layout, portNameToNewShape);
				DiagnosticTrace.Step($"build_nuclear_reactor[{newIdStr}]: fuel-port shapes substituted " +
					$"(in={inputShape?.Id.Value}, out={outputShape?.Id.Value}; per-name).");
			} else {
				DiagnosticTrace.Step($"build_nuclear_reactor[{newIdStr}]: substitution checked, no changes needed " +
					$"(source FuelInPort='{src.FuelInPort}', FuelOutPort='{src.FuelOutPort}'; " +
					$"layout has {src.Layout.Ports.Length} port(s)).");
			}

			// layout_str override — replaces the source layout string
			// entirely when the modder swapped the reactor prefab/model
			// and needs a different tile shape. Re-parsed via the live
			// LayoutParser so derived state stays self-consistent.
			if (args.GetArgument<string>("layout_str").WhenExists(out string layoutStrOverride)
					&& !string.IsNullOrWhiteSpace(layoutStrOverride)) {
				string[] overrideLines = sanitizeLayoutLines(layoutStrOverride);
				reactorLayout = registrator.LayoutParser.ParseLayoutOrThrow(paramsWithCustomTokens(src.Layout), overrideLines);
				DiagnosticTrace.Step($"build_nuclear_reactor[{newIdStr}]: layout_str override applied " +
					$"({overrideLines.Length} line(s), {reactorLayout.Ports.Length} port(s)).");
			}

			// add_ports — extra ports beyond the source built-ins.
			// Same layoutWithExtraPorts helper edit_machine_ports /
			// clone_machine use. Reactor runtime only acts on the
			// named fuel/water/steam/coolant ports — extras are
			// visual / connectable but functionally inert.
			if (args.GetArgument<List<object>>("add_ports").WhenExists(out var rawExtras)
					&& rawExtras.Count > 0) {
				var extraPorts = parsePortListStatic(registrator.PrototypesDb, args, "add_ports");
				if (extraPorts.Count > 0) {
					reactorLayout = layoutWithExtraPorts(reactorLayout, extraPorts);
					DiagnosticTrace.Step($"build_nuclear_reactor[{newIdStr}]: add_ports applied " +
						$"({extraPorts.Count} extra port(s); total {reactorLayout.Ports.Length}).");
				}
			}

			// ---- Fluid overrides ------------------------------------
			// Each pair (product + port-char + optional shape) falls back
			// to the source's value when not supplied. Port-shape overrides
			// go through the same layoutWithSubstitutedPortShapes path the
			// fuel ports use so port-name-to-shape mappings stay consistent.
			Mafi.Core.Products.ProductProto coolantInProto = args.GetArgument<Mafi.Core.Products.ProductProto>("coolantIn")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseDefault(src.CoolantIn);
			Mafi.Core.Products.ProductProto coolantOutProto = args.GetArgument<Mafi.Core.Products.ProductProto>("coolantOut")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseDefault(src.CoolantOut);
			char coolantInPortChar = singleCharOrDefault(
				args.GetArgument<string>("coolantInPort").ElseDefault(null), src.CoolantInPort);
			char coolantOutPortChar = singleCharOrDefault(
				args.GetArgument<string>("coolantOutPort").ElseDefault(null), src.CoolantOutPort);

			string coolantInShapeIdOverride  = args.GetArgument<string>("coolantInPortShape").ElseDefault(null);
			string coolantOutShapeIdOverride = args.GetArgument<string>("coolantOutPortShape").ElseDefault(null);
			Mafi.Core.Ports.Io.IoPortShapeProto coolantInShape =
				!string.IsNullOrEmpty(coolantInShapeIdOverride)
					? resolvePortShape(registrator.PrototypesDb, coolantInShapeIdOverride)
					: null;
			Mafi.Core.Ports.Io.IoPortShapeProto coolantOutShape =
				!string.IsNullOrEmpty(coolantOutShapeIdOverride)
					? resolvePortShape(registrator.PrototypesDb, coolantOutShapeIdOverride)
					: null;
			if (coolantInShape != null || coolantOutShape != null) {
				var coolantPortNameToNewShape = new Dictionary<char, Mafi.Core.Ports.Io.IoPortShapeProto>();
				if (coolantInShape != null && coolantInPortChar != default(char))
					coolantPortNameToNewShape[coolantInPortChar] = coolantInShape;
				if (coolantOutShape != null && coolantOutPortChar != default(char))
					coolantPortNameToNewShape[coolantOutPortChar] = coolantOutShape;
				if (coolantPortNameToNewShape.Count > 0) {
					reactorLayout = layoutWithSubstitutedPortShapes(reactorLayout, coolantPortNameToNewShape);
				}
			}

			// Water-in / steam-out per power level. Either half can be
			// overridden independently — product-only override keeps the
			// source's quantity, quantity-only override keeps the source's
			// product. Earlier revision required BOTH halves which
			// silently discarded product-only overrides ("water is not
			// replaced" symptom).
			Mafi.Core.Products.ProductProto waterInProto = args.GetArgument<Mafi.Core.Products.ProductProto>("waterInProduct")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseNull();
			int? waterInQty = args.GetArgument<int>("waterInQuantity").WhenExists(out int wq) ? (int?)wq : null;
			Mafi.Core.ProductQuantity waterInPq = (waterInProto != null || waterInQty.HasValue)
				? new Mafi.Core.ProductQuantity(
					waterInProto ?? src.WaterInPerPowerLevel.Product,
					new Quantity(waterInQty ?? src.WaterInPerPowerLevel.Quantity.Value))
				: src.WaterInPerPowerLevel;

			Mafi.Core.Products.ProductProto steamOutProto = args.GetArgument<Mafi.Core.Products.ProductProto>("steamOutProduct")
				.When<Mafi.Core.Products.ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>((Proto.ID)id))
				.When<string>(id => registrator.PrototypesDb.GetOrThrow<Mafi.Core.Products.ProductProto>(new Mafi.Core.Products.ProductProto.ID(id)))
				.ElseNull();
			int? steamOutQty = args.GetArgument<int>("steamOutQuantity").WhenExists(out int sq) ? (int?)sq : null;
			Mafi.Core.ProductQuantity steamOutPq = (steamOutProto != null || steamOutQty.HasValue)
				? new Mafi.Core.ProductQuantity(
					steamOutProto ?? src.SteamOutPerPowerLevel.Product,
					new Quantity(steamOutQty ?? src.SteamOutPerPowerLevel.Quantity.Value))
				: src.SteamOutPerPowerLevel;

			string waterInPortsStr  = args.GetArgument<string>("waterInPorts").ElseDefault(src.WaterInPorts);
			string steamOutPortsStr = args.GetArgument<string>("steamOutPorts").ElseDefault(src.SteamOutPorts);

			// ---- Enrichment override --------------------------------
			// Modder passes an Enrichment(...) struct OR omits the arg
			// entirely. When supplied, every non-inherit field replaces
			// the source's EnrichmentData; the rest pass through
			// untouched. When the source has no Enrichment and the
			// modder didn't supply one, the cloned reactor inherits
			// None (i.e. no breeding).
			Mafi.Option<NuclearReactorProto.EnrichmentData> enrichmentOverride = src.Enrichment;
			if (args["enrichment"]?.Value is Enrichment py) {
				enrichmentOverride = buildEnrichmentDataFromPython(py, src.Enrichment);
			}

			StaticEntityProto.ID newId = new StaticEntityProto.ID(newIdStr);
			NuclearReactorProto clone = new NuclearReactorProto(
				id:                       newId,
				strings:                  Proto.CreateStr(newId, name, desc, null),
				layout:                   reactorLayout,
				costs:                    src.Costs,
				maxPowerLevel:            maxPowerLevel,
				fuelCapacity:             new Quantity(fuelCapacity),
				minFuelToOperate:         new Quantity(minFuel),
				waterInPerStep:           waterInPq,
				steamOutPerStep:          steamOutPq,
				waterInPorts:             waterInPortsStr,
				steamOutPorts:            steamOutPortsStr,
				processDuration:          Duration.FromSec(durationSec),
				fuelPairs:                sourceFuelPairs,
				fuelInPort:               src.FuelInPort,
				fuelOutPort:              src.FuelOutPort,
				coolantIn:                coolantInProto,
				coolantOut:               coolantOutProto,
				coolantInPort:            coolantInPortChar,
				coolantOutPort:           coolantOutPortChar,
				leakRadiationOnMeltdown:  src.LeakRadiationOnMeltdown,
				destroyFuelOnMeltdown:    src.DestroyFuelOnMeltdown,
				computingConsumed:        new Mafi.Computing(computing),
				enrichment:               enrichmentOverride,
				graphics:                 src.Graphics);
			ResearchNodeProto research = resolveResearch(registrator, args);
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			wireProtoUnlock(registrator, clone, clone, research, locked);
			return clone;
		});
	}

	// build_machine constructor body. Lives in its own static method so the
	// resolver dictionary stays scannable — the ctor itself is long enough
	// that inlining it would crowd the dict.
	// define_box_type / layout_token — register a custom layout tile token
	// for the active pack. The token feeds COI's layout parser (via
	// paramsWithCustomTokens) so authored layout_str grids can paint with
	// modder-defined tiles. Returns null (no proto produced).
	private static Constructor makeDefineBoxTypeCtor() {
		return new Constructor([
			"boxTypeId", "token", "heightFrom", "heightTo",
			"constraint", "surface", "terrainMaterial", "isRamp",
		], args => {
			string token = args.GetArgument<string>("token").ElseRequiredThrow();
			if (token == null || token.Length != 3) {
				throw new ArgumentException(
					$"define_box_type: token '{token}' must be exactly 3 characters.");
			}
			char first = token[0];
			if ((first >= 'A' && first <= 'Z')
					|| first == '^' || first == '>' || first == 'v' || first == '<' || first == '+') {
				throw new ArgumentException(
					$"define_box_type: token '{token}' first character '{first}' collides with a " +
					"port name (A-Z) or direction (^ > v < +); pick a different leading character.");
			}

			int heightFrom = args.GetArgument<int>("heightFrom").ElseDefault(0);
			int? heightTo = null;
			if (args.GetArgument<int>("heightTo").WhenExists(out int htv)) heightTo = htv;

			LayoutTileConstraint constraint = LayoutTileConstraint.None;
			if (args.GetArgument<string>("constraint").WhenExists(out string constraintStr)
					&& !string.IsNullOrEmpty(constraintStr)) {
				Enum.TryParse(constraintStr, true, out constraint);
			}

			Proto.ID? toProtoId(string argName) {
				object v = args[argName]?.Value;
				if (v == null) return null;
				if (v is Proto.ID pid) return pid;
				if (v is string s && s.Length > 0) return new Proto.ID(s);
				return null;
			}
			Proto.ID? surfaceId  = toProtoId("surface");
			Proto.ID? materialId = toProtoId("terrainMaterial");
			bool isRamp = args.GetArgument<bool>("isRamp").ElseDefault(false);

			CustomLayoutToken ct = new CustomLayoutToken(token, (p, h) => {
				// h is the per-tile wildcard digit (0 when the token carries no
				// '0' slot / exact match). Default the exclusive ceiling to the
				// digit, or heightFrom+1 for a fixed 1-tall token.
				int to = heightTo ?? (h > 0 ? h : heightFrom + 1);
				return new LayoutTokenSpec(heightFrom, to, constraint,
					null, null, null, null, materialId, surfaceId, isRamp, false, 0);
			});

			if (!s_customTokens.TryGetValue(s_currentModId, out List<CustomLayoutToken> list)) {
				list = new List<CustomLayoutToken>();
				s_customTokens[s_currentModId] = list;
			}
			list.Add(ct);
			DiagnosticTrace.Step($"define_box_type[{s_currentModId}]: token '{token}' registered " +
				$"(heightFrom={heightFrom}, heightTo={(heightTo.HasValue ? heightTo.Value.ToString() : "wildcard")}).");
			return null;
		});
	}

	private static Constructor makeBuildMachineCtor(ProtoRegistrator registrator) {
		return new Constructor([
			"machineId",
			"source",
			"name",
			"description",
			"ports",
			"add_ports",  // legacy alias for `ports`; kept so packs predating the rename still load
			"consumedPowerPerTick",
			"research",
			"copy_recipes",
			"copy_layout",
			"copy_ports",
			"copy_graphics",
			"layout_str",  // str — optional, authored footprint that overrides copy_layout
			"lockedOnInit",
		], args => {
			// Required-arg sanity checks.
			if (args["machineId"]?.Value == null) {
				throw new ArgumentException(
					"build_machine: required argument `machineId` is missing or None. " +
					"Pass a MachineProto.ID or a string id for the new machine.");
			}
			if (args["source"]?.Value == null) {
				throw new ArgumentException(
					"build_machine: required argument `source` is missing or None. " +
					"Pass the MachineProto / MachineProto.ID / string id of the machine to clone.");
			}
			string newIdStr = args.GetArgument<string>("machineId")
				.When<Proto.ID>(p => p.Value)
				.When<MachineProto.ID>(p => p.Value)
				.ElseRequiredThrow();

			MachineProto sourceProto = args.GetArgument<MachineProto>("source")
				.When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
				.When<string>(s => registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(s)))
				.ElseRequiredThrow();

			string name = args.GetArgument<string>("name")
				.ElseDefault(sourceProto.Strings.Name.TranslatedString ?? sourceProto.Id.Value);
			string desc = args.GetArgument<string>("description")
				.ElseDefault(sourceProto.Strings.DescShort.TranslatedString ?? "");

			// Prefer the new `ports=` name; fall back to the legacy
			// `add_ports=` so packs predating the rename keep loading.
			// Either-or, not merged: a pack that passes both gets the
			// canonical `ports=` value and silently ignores the legacy
			// alias to keep the precedence rule obvious.
			List<Mafi.Core.Ports.Io.IoPortTemplate> extras =
				parsePortListStatic(registrator.PrototypesDb, args, "ports");
			if (extras.Count == 0) {
				extras = parsePortListStatic(registrator.PrototypesDb, args, "add_ports");
			}

			// Three independent copy toggles, all defaulting to TRUE so the
			// behaviour matches the original "clone everything from source"
			// contract when modders don't pass them.
			//   copy_layout   - reuse source's tile shape + terrain
			//                   footprint. When false the new machine starts
			//                   from a 1x1 empty layout and the modder must
			//                   supply ports via add_ports for it to be
			//                   functional.
			//   copy_ports    - keep the source layout's existing ports.
			//                   When false they're stripped; only add_ports
			//                   contributes to the new machine.
			//   copy_graphics - reuse source's Gfx (prefab path, sounds,
			//                   particles, sign, toolbar categories). When
			//                   false MachineProto.Gfx.Empty is used so
			//                   modders can attach a custom prefab later
			//                   without inheriting the source's look.
			bool copyLayout   = args.GetArgument<bool>("copy_layout").ElseDefault(true);
			bool copyPorts    = args.GetArgument<bool>("copy_ports").ElseDefault(true);
			bool copyGraphics = args.GetArgument<bool>("copy_graphics").ElseDefault(true);

			// Resolve the base layout the new machine starts from. Priority:
			//   layout_str  → parse the authored footprint (OVERRIDES copy_layout);
			//                 custom box types defined in this pack are made
			//                 available to the parser via paramsWithCustomTokens.
			//   copy_layout → reuse the source's tile shape + terrain footprint.
			//   neither     → a minimal 1x1 empty layout the modder ports up.
			bool hasLayoutStr = args.GetArgument<string>("layout_str").WhenExists(out string layoutStrOverride)
				&& !string.IsNullOrWhiteSpace(layoutStrOverride);
			EntityLayout baseLayout;
			if (hasLayoutStr) {
				string[] overrideLines = sanitizeLayoutLines(layoutStrOverride);
				baseLayout = registrator.LayoutParser.ParseLayoutOrThrow(
					paramsWithCustomTokens(sourceProto.Layout), overrideLines);
				DiagnosticTrace.Step($"build_machine[{newIdStr}]: layout_str override applied " +
					$"({overrideLines.Length} line(s), {baseLayout.Ports.Length} port(s))");
			} else if (copyLayout) {
				baseLayout = sourceProto.Layout;
			} else {
				baseLayout = EntityLayout.CreateEmpty(RelTile2i.Zero, RelTile2i.Zero);
			}

			// Source-side port set the new layout starts with, before any
			// extras get merged in. Kept only when copy_ports is on AND we
			// have a real footprint (copy_layout or an authored layout_str).
			// When copy_ports=False the editor side imports the source ports
			// into add_ports for in-list editing, so every modification lives
			// in the same list the modder is already looking at.
			bool keepBasePorts = copyPorts && (copyLayout || hasLayoutStr);
			ImmutableArray<Mafi.Core.Ports.Io.IoPortTemplate> sourcePorts =
				keepBasePorts ? baseLayout.Ports
				              : ImmutableArray<Mafi.Core.Ports.Io.IoPortTemplate>.Empty;

			// Duplicate-name check applies whether the conflict is with
			// kept base ports or with another entry in the extras list
			// itself (e.g. two add_ports rows declaring port 'A').
			var seenNames = new HashSet<char>(sourcePorts.Select(p => p.Name));
			foreach (var p in extras) {
				if (!seenNames.Add(p.Name)) {
					throw new ArgumentException(
						$"build_machine[{newIdStr}]: port name '{p.Name}' is duplicated " +
						(keepBasePorts
							? "between the layout and `add_ports`."
							: "in `add_ports`."));
				}
			}

			// Apply ports onto the base layout. copy_ports keeps the base
			// layout's own ports (plus extras); otherwise extras replace them.
			EntityLayout layout;
			if (copyPorts) {
				layout = extras.Count > 0 ? layoutWithExtraPorts(baseLayout, extras) : baseLayout;
			} else {
				layout = layoutWithReplacedPorts(baseLayout, extras);
			}

			// copyGraphics=true: clone the source's Gfx into a fresh
			// instance with customIconPath bound to the source's IconPath.
			// Sharing sourceProto.Graphics directly would let Initialize
			// overwrite IconPath + m_proto on both protos (see
			// cloneMachineGfx's preamble for the full incident report).
			MachineProto.Gfx graphics = copyGraphics
				? cloneMachineGfx(sourceProto)
				: MachineProto.Gfx.Empty;

			Mafi.Electricity powerOverride = args.GetArgument<Mafi.Electricity>("consumedPowerPerTick")
				.When<int>(kw => Mafi.Electricity.FromKw(kw))
				.ElseDefault(sourceProto.ConsumedPowerPerTick);

			DiagnosticTrace.Step($"build_machine[{newIdStr}]: source={sourceProto.Id.Value}, " +
				$"extra_ports={extras.Count}, total_ports={layout.Ports.Length}, " +
				$"copy_layout={copyLayout}, copy_ports={copyPorts}, copy_graphics={copyGraphics}");

			MachineProto.ID newId = new MachineProto.ID(newIdStr);
			MachineProto clone = new MachineProto(
				id:                                 newId,
				strings:                            Proto.CreateStr(newId, name, desc, null),
				layout:                             layout,
				costs:                              sourceProto.Costs,
				consumedPowerPerTick:               powerOverride,
				computingConsumed:                  sourceProto.ComputingConsumed,
				buffersMultiplier:                  sourceProto.BuffersMultiplier,
				useAllRecipesAtStartOrAfterUnlock:  sourceProto.UseAllRecipesAtStartOrAfterUnlock,
				animationParams:                    sourceProto.AnimationParams,
				graphics:                           graphics,
				emissionWhenRunning:                sourceProto.EmissionWhenRunning,
				isWasteDisposal:                    sourceProto.IsWasteDisposal,
				disableLogisticsByDefault:          sourceProto.DisableLogisticsByDefault,
				boostCost:                          sourceProto.BoostCost,
				tags:                               null);

			bool copyRecipes = args.GetArgument<bool>("copy_recipes").ElseDefault(true);
			if (copyRecipes) {
				foreach (RecipeProto r in sourceProto.Recipes.AsEnumerable()) {
					clone.AddRecipe(r);
				}
			}

			ResearchNodeProto research = args.GetArgument<ResearchNodeProto>("research")
				.When<ResearchNodeProto.ID>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(rid))
				.When<string>(rid => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(rid)))
				.ElseNull();
			bool locked = args.GetArgument<bool>("lockedOnInit").ElseDefault(research != null);
			registrator.PrototypesDb.Add(clone, locked);

			if (research != null) {
				typeof(ResearchNodeProto).GetField("<Units>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
					?.SetValue(research, research.Units
						.AsEnumerable()
						.Concat(new IUnlockNodeUnit[] { new ProtoWithIconUnlock(clone, false) })
						.ToImmutableArray());
				typeof(ResearchNodeProto.Gfx).GetField("<IconsProtos>k__BackingField",
						BindingFlags.NonPublic | BindingFlags.Instance)
					?.SetValue(research.Graphics, research.Graphics.IconsProtos
						.AsEnumerable()
						.Concat(new IProtoWithIcon[] { clone })
						.Distinct()
						.ToImmutableArray());
			}

			DiagnosticTrace.Step($"build_machine[{newIdStr}]: complete (locked={locked})");
			return clone;
		});
	}

	// Layered layout-override pipeline shared by every entity-clone
	// build_* ctor. Three stages, applied in order:
	//   1. Start from the source proto's layout.
	//   2. If `layout_str` is set, REPLACE the layout string entirely
	//      (re-parsed through the live LayoutParser with the source's
	//      LayoutParams — so vehicle heights / placement rules stay
	//      the source's).
	//   3. If `add_ports` is set, append those PortRef structs onto
	//      whatever layout came out of step 2 via layoutWithExtraPorts.
	// Each step logs a DiagnosticTrace line so the modder can see what
	// transformations ran. Returns the final EntityLayout.
	// Normalize a layout_str into the rectangular grid Mafi's LayoutParser
	// expects. Mafi rejects ANY mismatch between line[i].Length and line
	// [0].Length (after a "% 3 == 0" check), so spurious leading/trailing
	// whitespace on a single line — common after editor textarea edits —
	// fails the whole pack with "Length 51 of line 16 does not match
	// layout line length 48". Three contiguous spaces tokenize as the
	// "void" token in Mafi (see EntityLayoutParser line 259), so adding
	// or removing whitespace around the actual content is safe — it just
	// shifts where the implicit void cells live without changing real
	// tile coords.
	//
	// Steps:
	//   1. Split on CR/LF.
	//   2. Right-trim every line (strips any accumulated trailing space).
	//   3. Canonical width = line 0's length (matches Mafi's own rule).
	//      Bumped up to a multiple of 3 if line 0 itself wasn't aligned.
	//   4. For each subsequent line:
	//        - Longer and the excess is leading whitespace → left-trim
	//          exactly the excess (preserves real content position).
	//        - Shorter → right-pad with spaces to canonical (adds void
	//          tokens to fill).
	//        - Otherwise leave alone — Mafi's own error message will
	//          point at the bad line with the original length info.
	//   5. Log a warning when any normalization happened so the modder
	//      can audit the layout for the underlying typo.
	private static string[] sanitizeLayoutLines(string layoutStr) {
		string[] raw = layoutStr.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
		for (int i = 0; i < raw.Length; i++) {
			raw[i] = raw[i].TrimEnd();
		}
		if (raw.Length == 0) return raw;

		int canonical = raw[0].Length;
		if (canonical % 3 != 0) {
			// Round line 0 up to nearest multiple of 3 so it parses at all.
			int padded = ((canonical / 3) + 1) * 3;
			raw[0] = raw[0].PadRight(padded);
			canonical = padded;
		}

		int normalized = 0;
		for (int i = 1; i < raw.Length; i++) {
			int len = raw[i].Length;
			if (len == canonical) continue;
			if (len > canonical) {
				int excess = len - canonical;
				// Only safe to left-trim if every leading character we'd
				// drop is whitespace — otherwise we'd silently delete
				// real tile content. Same idea for trailing: if the line
				// still has trailing whitespace after the earlier
				// TrimEnd (e.g. line ended in tabs that we left alone),
				// try trimming there. We do trailing first because it's
				// more common and lower-risk.
				if (raw[i].Substring(canonical, excess).Trim().Length == 0) {
					raw[i] = raw[i].Substring(0, canonical);
					normalized++;
				} else if (raw[i].Substring(0, excess).Trim().Length == 0) {
					raw[i] = raw[i].Substring(excess);
					normalized++;
				}
				// else: real content sits outside canonical → leave it
				// alone, Mafi will throw the original mismatch error.
			} else {
				raw[i] = raw[i].PadRight(canonical);
				normalized++;
			}
		}

		if (normalized > 0) {
			Log.Warning($"layout_str sanitized: normalized {normalized} line(s) to canonical width {canonical}. " +
				"Check the source layout for spurious leading/trailing whitespace.");
		}
		return raw;
	}

	private static EntityLayout buildLayoutFromOverrides(
			ProtoRegistrator registrator,
			EntityLayout sourceLayout,
			Constructor.CallArguments args,
			string fnNameForTrace, string newIdStr) {
		EntityLayout layout = sourceLayout;

		if (args.GetArgument<string>("layout_str").WhenExists(out string layoutStrOverride)
				&& !string.IsNullOrWhiteSpace(layoutStrOverride)) {
			string[] overrideLines = sanitizeLayoutLines(layoutStrOverride);
			layout = registrator.LayoutParser.ParseLayoutOrThrow(paramsWithCustomTokens(sourceLayout), overrideLines);
			DiagnosticTrace.Step($"{fnNameForTrace}[{newIdStr}]: layout_str override applied " +
				$"({overrideLines.Length} line(s), {layout.Ports.Length} port(s))");
		}

		if (args.GetArgument<List<object>>("add_ports").WhenExists(out var rawExtras)
				&& rawExtras.Count > 0) {
			var extraPorts = parsePortListStatic(registrator.PrototypesDb, args, "add_ports");
			if (extraPorts.Count > 0) {
				layout = layoutWithExtraPorts(layout, extraPorts);
				DiagnosticTrace.Step($"{fnNameForTrace}[{newIdStr}]: add_ports applied " +
					$"({extraPorts.Count} extra port(s); total {layout.Ports.Length})");
			}
		}

		return layout;
	}

	// Static-context wrapper around the instance method parsePortList so
	// makeMachineCloneCtor (which is itself static) can reach it. We pass
	// the ProtosDb directly and rely on buildIoPortTemplate (already
	// static) for the per-Port resolution.
	private static List<Mafi.Core.Ports.Io.IoPortTemplate> parsePortListStatic(
		ProtosDb db, Constructor.CallArguments args, string argName)
	{
		var result = new List<Mafi.Core.Ports.Io.IoPortTemplate>();
		if (!args.GetArgument<List<object>>(argName).WhenExists(out var raw)) return result;
		foreach (object item in raw) {
			if (item is Port p) {
				result.Add(buildIoPortTemplate(db, p));
			} else {
				throw new ArgumentException($"'{argName}' list item must be Port, got {item?.GetType()?.FullName ?? "null"}.");
			}
		}
		return result;
	}
}

public static class ResearchPositionExtension {

	public static void GridPositionWherePossible(this ResearchNodeProto proto, ProtosDb protos, params Vector2i[] options) {
		Vector2i defaultOption = options[0];
		Dict<int, Lyst<int>> usedOptions = protos.All<ResearchNodeProto>()
			.SelectMany(p => yieldFrom(p.GridPosition)
				.GroupBy(v => v.X, v => v.Y))
			.GroupBy(g => g.Key, g => g)
			.ToDict(p => p.Key, p => p.SelectMany(sg => sg).ToLyst());

		// First pass: try each requested option as-is. If any is free, place there
		// and we're done — no need to enter the fallback search loop below.
		bool placed = false;
		foreach (Vector2i option in options) {
			if (usedOptions.TryGetValue(option.X, out Lyst<int> yS)
				&& (yS.Contains(option.Y)
					|| yS.Contains(option.Y + 1)
					|| yS.Contains(option.Y + 2))) {
				continue;
			}
			proto.GridPosition = option;
			placed = true;
			break;
		}
		if (placed) return;

		// Fallback: walk outward (yP increases, yN decreases) from defaultOption.Y
		// looking for a free Y slot in the defaultOption.X column. Bounded with a
		// hard iteration cap so a bug (or an empty-column case) can't infinite-loop
		// the mod loader — this is what previously hung the game.
		const int FALLBACK_MAX_ITER = 1000;
		for (int yP = defaultOption.Y, yN = defaultOption.Y, iter = 0;
			iter < FALLBACK_MAX_ITER;
			yP++, yN--, iter++) {
			bool positiveUsed = usedOptions.TryGetValue(defaultOption.X, out Lyst<int> yS)
				&& yS.Contains(yP)
				&& yS.Contains(yP + 1)
				&& yS.Contains(yP + 2);
			bool negativeUsed = usedOptions.TryGetValue(defaultOption.X, out yS)
				&& yS.Contains(yN)
				&& yS.Contains(yN + 1)
				&& yS.Contains(yN + 2);
			if (positiveUsed && negativeUsed) {
				continue;
			}
			if (positiveUsed) {
				proto.GridPosition = new Vector2i(defaultOption.X, yN);
				return;
			}
			if (negativeUsed) {
				proto.GridPosition = new Vector2i(defaultOption.X, yP);
				return;
			}
			// Both Y slots free at this column position — just take the defaultOption
			// (which would have been preferable in the first place but we missed in
			// the first loop above; this is the case that used to infinite-loop).
			proto.GridPosition = defaultOption;
			return;
		}
		// Safety net: if the iteration cap was hit, fall back to defaultOption rather
		// than leaving GridPosition at its default value.
		Log.Warning($"GridPositionWherePossible: iteration cap reached for '{proto.Id.Value}', " +
			$"using default position {defaultOption}.");
		proto.GridPosition = defaultOption;
	}

	private static IEnumerable<Vector2i> yieldFrom(Vector2i gridPosition) {
		for (int x = gridPosition.X; x < gridPosition.X + 4; x++) {
			for (int y = gridPosition.Y; y < gridPosition.Y + 3; y++) {
				yield return new Vector2i(x, y);
			}
		}
	}
}


public static class CategoriesExtensions {
	extension(LayoutEntityProto.Gfx graphics) {
		public void ExtendCategories(ToolbarCategoryProto category) {
			ImmutableArray<ToolbarEntryData> categories = graphics.Categories;
			categories = categories.IsNotValidOrEmpty
				? ImmutableArray.Create(new ToolbarEntryData(category))
				: categories.Concat(ImmutableArray.Create(new ToolbarEntryData(category)));

			Type graphicsType = typeof(LayoutEntityProto.Gfx);
			FieldInfo fieldInfo = graphicsType.GetField("<Categories>k__BackingField",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			if (fieldInfo is null) {
				throw new NullReferenceException("Cannot find backing field for graphics");
			}
			fieldInfo!.SetValue(graphics, categories);
		}
	}
}

public class LoadAction : IEnumerator {

	public static readonly LoadAction Done = new ();
	public static readonly LoadAction Failed = new (failed: true);
	public static LoadAction WaitFor(Func<bool> waiting) {
		return new LoadAction(waiting);
	}

	public bool IsFailed { get; }
	public bool IsCompleted => IsFailed || m_awaitAction == null || m_awaitAction.Invoke();

	private readonly Func<bool> m_awaitAction;

	public object Current => throw new NotImplementedException();

	private LoadAction(Func<bool> awaitAction = null, bool failed = false) {
		m_awaitAction = awaitAction;
		IsFailed = failed;
	}

	public bool MoveNext() {
		return m_awaitAction.Invoke();
	}

	public void Reset() {
	}
}

public struct Product {
	public string port;
	public ProductProto product;
	public Quantity quantity;
}

public struct FuelPair {
	public Mafi.Core.Products.ProductProto fuelIn;
	public Mafi.Core.Products.ProductProto spentFuelOut;
	public int durationSeconds;
}

// Python-side Enrichment(...) factory result. Surfaces the source
// NuclearReactorProto.EnrichmentData fields as nullable overrides so a
// modder can author a partial breeding patch: every non-null field
// replaces the source's value at the runtime conversion step, null
// fields fall through to whatever the source had.
public struct Enrichment {
	public Mafi.Core.Products.ProductProto inputProduct;
	public char inPort;            // default('\0') -> inherit from source
	public Mafi.Core.Products.ProductProto outputProduct;
	public char outPort;           // default('\0') -> inherit
	// processedPerLevel as numerator/denominator so PartialQuantity
	// fractional rates (e.g. 1.2 per step) round-trip exactly. Both -1
	// = inherit.
	public int processedPerLevelNumerator;
	public int processedPerLevelDenominator;
	public int buffersCapacity;   // -1 -> inherit
	// Bool overrides packed as a tri-state int: -1 = inherit, 0 = false,
	// 1 = true. Plain bool can't disambiguate "not supplied" from
	// "supplied as false".
	public int destroyContentOnMeltdown;
	public int defaultEnrichmentStep; // -1 -> inherit
	public System.Collections.Generic.List<EnrichmentStep> steps;
}

// Python-side EnrichmentStep(fuelMultiplierPercent, breedingRatio,
// steamReductionDiv) — one row of the enrichment curve.
public struct EnrichmentStep {
	public int fuelMultiplierPercent; // 100 = 1x; emitter rounds to nearest int %
	public int breedingRatio;
	public int steamReductionDiv;
}

public struct Port {
	public char name;
	public string type;
	public string shape;
	public Vector3i position;
	public string direction;
	public bool canOnlyConnectToTransports;
}
