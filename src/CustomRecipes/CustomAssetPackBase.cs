using System;
using System.Reflection;
using System.Reflection.Emit;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;

namespace CustomAssets
{
	// Abstract pack-entry implementation. All pack-side logic lives here so it
	// can be maintained in one place (the core CustomAssets.dll) and a pack-side
	// DLL only needs to ship a minimal sealed concrete subclass that Mafi can
	// instantiate by name via primary_mod_class_name.
	//
	// A pack's CustomAssetPack.dll loads under non_locking_dll_load — each load
	// creates a fresh CLR Assembly instance, so each pack's concrete subclass
	// gets a distinct System.Type identity. The abstract base, the registrator,
	// and all the runtime helpers all resolve at runtime to the SINGLE base mod
	// CustomAssets.dll already loaded via mod_dependencies.
	//
	// RegisterDependencies also emits a per-pack marker Type named after
	// Manifest.Id (inheriting EmittedPackMod) and registers it with the
	// DependencyResolverBuilder — preparatory infrastructure for code that may
	// later enumerate loaded packs from DI by their manifest-id-named type.
	public abstract class CustomAssetPackBase : IMod {

		public ModManifest Manifest { get; set; }
		public bool IsUiOnly { get; }
		public Option<IConfig> ModConfig { get; }
		public ModJsonConfig JsonConfig { get; }

		protected CustomAssetPackBase(ModManifest manifest) {
			Log.Info($"{manifest.Id}: pack constructed (CustomAssetPack entry)");
			Manifest = manifest;
			JsonConfig = new ModJsonConfig(this);
		}

		public void EarlyInit(DependencyResolver resolver) {
			// Nothing to do — the marker instance is constructed and registered
			// during RegisterDependencies, while we still have the builder.
		}

		public void Initialize(DependencyResolver resolver, bool gameWasLoaded) {
			// Nothing to do — see EarlyInit. Trying to resolver.Instantiate the
			// emitted type here is too late: TerrainManager.initAfterLoad
			// requests AllImplementationsOf<IMod> during save load (before
			// Initialize), and DI tries to construct every IMod registration
			// via ctor injection — which fails because ModManifest is not a
			// DI service. Pre-built RegisterInstance avoids that path entirely.
		}

		public void RegisterPrototypes(ProtoRegistrator registrator) {
			Log.Info($"{Manifest.Id}: pack registering prototypes");

			// migrate_recipe() tombstones buffer up while the pack's .py files run and
			// are handed to the game only once the whole pack has loaded — the "is the
			// old recipe still registered?" test has to see the pack's FINAL proto set.
			// Bracketed here rather than inside RegisterData so the buffer's lifetime is
			// owned by the pack entry point, and so a pack that fails to load can be
			// reported instead of silently dropping its migrations. This method lives in
			// the core CustomAssets.dll, so packs pick the behaviour up without shipping
			// a new CustomAssetPack.dll.
			RecipeMigrationBuffer.Reset(Manifest.Id);
			try {
				new CustomAssetRegistrator().RegisterData(registrator);
			} catch {
				// A failed pack does NOT abort the game — it just doesn't apply. Its
				// recipes are therefore absent, so registering migrations now would
				// only produce "target not registered" noise. Warn loudly instead: the
				// player is about to play, and possibly SAVE, with the pack's recipes
				// resolving to phantoms that Machine.initSelf strips on the next load.
				RecipeMigrationBuffer.Abandon(Manifest.Id);
				throw;
			}
			RecipeMigrationBuffer.Flush(Manifest.Id, registrator);
		}

		public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool gameWasLoaded) {
			try {
				emitAndRegisterPackMarker(depBuilder, Manifest, JsonConfig);
			} catch (Exception ex) {
				Log.Warning($"{Manifest.Id}: emit+register pack marker failed: {ex.GetType().Name}: {ex.Message}");
			}
		}

		public void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues) {
			// TODO define migration.py
		}

		public void Dispose() {
			// Nothing to dispose
		}

		private static void emitAndRegisterPackMarker(DependencyResolverBuilder depBuilder, ModManifest manifest, ModJsonConfig jsonConfig) {
			// One dynamic assembly per pack so the assembly name is identifiable
			// in any diagnostic that walks AppDomain.GetAssemblies().
			AssemblyName asmName = new AssemblyName("CustomAssetPack.Emitted." + manifest.Id);
			AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(asmName.Name);

			// Type.Name = manifest.Id so DI introspection reads the pack identity
			// straight off the registered Type. Sealed because nothing should
			// subclass an emitted marker.
			TypeBuilder typeBuilder = moduleBuilder.DefineType(
				manifest.Id,
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
				typeof(EmittedPackMod));

			// public ctor(ModManifest m, ModJsonConfig j) : base(m, j) { }
			// EmittedPackMod's ctor is protected — accessible to subclasses in
			// any assembly (CLR accessibility is type-based, not assembly-based),
			// so the IL Call to it is valid.
			ConstructorInfo baseCtor = typeof(EmittedPackMod).GetConstructor(
				BindingFlags.Instance | BindingFlags.NonPublic,
				binder: null,
				types: [typeof(ModManifest), typeof(ModJsonConfig)],
				modifiers: null);
			ConstructorBuilder ctor = typeBuilder.DefineConstructor(
				MethodAttributes.Public | MethodAttributes.HideBySig
					| MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				CallingConventions.Standard,
				[typeof(ModManifest), typeof(ModJsonConfig)]);
			ILGenerator il = ctor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);       // this
			il.Emit(OpCodes.Ldarg_1);       // manifest
			il.Emit(OpCodes.Ldarg_2);       // jsonConfig
			il.Emit(OpCodes.Call, baseCtor!);
			il.Emit(OpCodes.Ret);

			Type emittedType = typeBuilder.CreateType();

			// Pre-construct the marker instance NOW with the manifest+jsonConfig
			// in hand. We MUST register an instance (not a type) because the
			// emitted ctor takes (ModManifest, ModJsonConfig) and ModManifest
			// is not a DI service — DI's auto-construction during
			// AllImplementationsOf<IMod> resolution would fail otherwise.
			IMod instance = (IMod)Activator.CreateInstance(emittedType, manifest, jsonConfig);
			depBuilder.RegisterInstance(instance).AsSelf().As<IMod>();

			Log.Info($"{manifest.Id}: emitted+registered marker type {emittedType.FullName}");
		}
	}
}
