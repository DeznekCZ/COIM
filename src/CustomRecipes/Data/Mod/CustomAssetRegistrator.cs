using CustomAssets.ModuleParser.Registrator;
using PythonAPI;
using CustomAssets.Utils;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities.Static;
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
using PythonAPI.Expressions;
using PythonAPI.Runtime;

namespace CustomAssets.Data.Mod;

public class CustomAssetRegistrator : IModData {

	private string m_modBasePath = ""; // SET IN RUNTIME, because of mod loading order

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
		m_configValues = ConfigLoader.Load(m_modBasePath);

		string modId = registrator.ActiveMod?.Manifest?.Id ?? "unknown-mod";
		DiagnosticTrace.Step($"RegisterData: start (mod={modId}, base={m_modBasePath})");

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
			DiagnosticTrace.Step($"RegisterData[{modId}]: __init__.py exists, loading");
			try {
				register(loaded, failed, registrator, initFile, modules);
				DiagnosticTrace.Step($"RegisterData[{modId}]: __init__.py loaded");
			} catch (Exception e) {
				DiagnosticTrace.Step($"RegisterData[{modId}]: __init__.py FAILED: {e.GetType().Name}: {e.Message}");
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
			DiagnosticTrace.Step($"RegisterData[{modId}]: enumerate-loading {enumerateFile.Name}");
			try {
				register(loaded, failed, registrator, enumerateFile, modules);
				DiagnosticTrace.Step($"RegisterData[{modId}]: enumerate-loaded {enumerateFile.Name}");
			} catch (Exception e) {
				string relPath = enumerateFile.FullName.Remove(0, modules.FullName.Length);
				DiagnosticTrace.Step($"RegisterData[{modId}]: {enumerateFile.Name} FAILED: {e.GetType().Name}: {e.Message}");
				Log.Error($"Failed to load definition: {relPath}");
				Log.Exception(e);
				failed.Add(enumerateFile.FullName);
				throw new CheckException(
					$"Failed to load definition '{relPath}': {e.Message}\n{e.StackTrace}", e);
			}
		}
		DiagnosticTrace.Step($"RegisterData[{modId}]: complete (loaded={loaded.Count}, failed={failed.Count})");

		// Note: we do NOT call CustomAssetManager.Instance.RunInjection() here. CAM may not yet
		// be constructed (it's lazy DI), and even if it were, it can't run before LPMM/
		// ProductsRenderer ctors anyway. Late injection for unit prefabs is handled by
		// CustomUnitPrefabHook, which depends on ProductsRenderer + CAM and patches the
		// renderer's cached CommonDataMutable for our products after both have constructed.
	}

	private void register(Set<string> loaded, Set<string> failed, ProtoRegistrator registrator, FileInfo file,
		DirectoryInfo modules) {
		DiagnosticTrace.Step($"register: parsing {file.Name}");
		Token[] tokens = Tokenizer.ParseFile(file.FullName);
		Block block = Lexer.Parse(tokens);
		DiagnosticTrace.Step($"register: parsed {file.Name} ({tokens.Length} tokens, {block.statements.Count} statements)");

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
			} catch (Exception ex) {
				DiagnosticTrace.Step($"register: {file.Name} statement[{stmtIndex}] THREW {ex.GetType().Name}: {ex.Message}");
				throw;
			}
			stmtIndex++;
		}

		loaded.Add(file.FullName);
		DiagnosticTrace.Step($"register: COMPLETED {file.Name}");
	}

	private Dictionary<string, object> resolvers(ProtoRegistrator registrator) {
		Dictionary<string, object> context = new Dictionary<string, object> {

			#region Mod config (config.json defaults — exposed as `config.<field>` to .py)

			["config"] = m_configValues,

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
				Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
				string assetPath = args.GetArgument<string>("path").ElseRequiredThrow();
				byte[] image = File.ReadAllBytes(Path.Combine(m_modBasePath, assetPath));
				if (!texture2D.LoadImage(image)) {
					throw new ArgumentException($"Could not load an image: {assetPath}");
				}

				if (args.GetArgument<string>("replace").WhenExists(out string replacementAssetPath)) {
					assetPath = replacementAssetPath;
				}

				texture2D.name = assetPath;
				CustomAssetManager.Alternations[assetPath] = texture2D;
				return new Tex { path = assetPath };
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
				 "width", "height", "depth", "mesh"],
				(args) => {
					string path = args.GetArgument<string>("path").ElseRequiredThrow();

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
						if (!CustomAssetManager.Meshes.TryGetValue(objRel, out mesh)) {
							string objFull = Path.Combine(m_modBasePath, objRel);
							mesh = ObjLoader.LoadFromFile(objFull);
							if (mesh != null) {
								mesh.name = objRel;
								CustomAssetManager.Meshes[objRel] = mesh;
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
				Texture2D texture = args.GetArgument<Texture2D>("texture")
					.When<string>(pathTex => {
						if (CustomAssetManager.Alternations.TryGetValue(pathTex, out UnityEngine.Object data)) {
							return data is Texture2D t2d
								? t2d
								: throw new ArgumentException($"Given prefab is not a Texture: {pathTex}");
						}

						Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
						string assetPath = pathTex;
						byte[] image = File.ReadAllBytes(Path.Combine(m_modBasePath, assetPath));
						if (!texture2D.LoadImage(image)) {
							throw new ArgumentException($"Could not load an image: {assetPath}");
						}
						CustomAssetManager.Alternations.Add(assetPath, texture2D);
						return texture2D;
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

				var color = args.GetArgument<ColorRgba>("color").ElseDefault(default);
				var transportColor = args.GetArgument<ColorRgba>("transportColor").ElseDefault(default);
				var transportAccentColor = args.GetArgument<ColorRgba>("transportAccentColor").ElseDefault(default);

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
				string[] layoutLines = newLayoutSrc.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
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
						case LayoutEntityProto lep: lep.Graphics.ExtendCategories(category); break;
						case MachineProto.ID machineID: registrator.PrototypesDb
								.GetOrThrow<MachineProto>(machineID)
								.Graphics.ExtendCategories(category);
							break;
						case StaticEntityProto.ID staticID: registrator.PrototypesDb
								.GetOrThrow<LayoutEntityProto>(staticID)
								.Graphics.ExtendCategories(category);
							break;
						case string strID: registrator.PrototypesDb
								.GetOrThrow<LayoutEntityProto>(new Proto.ID(strID))
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
		};
		return context;
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
