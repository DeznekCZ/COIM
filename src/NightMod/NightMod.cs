using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.Camera;
using NightMod.LampPosts;

namespace NightMod;

/// <summary>
/// Entry point for the Night Mod. It wires up the services that implement the day-night cycle,
/// exposes the player's <c>config.json</c> settings to them, and registers the lamp post building.
///
/// <list type="bullet">
///   <item><see cref="NightCycleManager"/> - simulation side, re-lights the weather conditions.</item>
///   <item><see cref="NightCycleRenderer"/> - rendering side, pushes the light into the scene.</item>
///   <item><see cref="LampPostProto"/> - a buildable powered lamp post that lights the ground at night.</item>
/// </list>
/// </summary>
public sealed class NightMod : IMod {

	public ModManifest Manifest { get; }

	/// <summary>The cycle changes solar power output, so this mod affects game state.</summary>
	public bool IsUiOnly => false;

	public Option<IConfig> ModConfig => Option<IConfig>.None;

	public ModJsonConfig JsonConfig { get; }

	public NightMod(ModManifest manifest) {
		Manifest = manifest;
		JsonConfig = new ModJsonConfig(this);
		Log.Info("NightMod: constructed");
	}

	public void RegisterPrototypes(ProtoRegistrator registrator) {
		// The lamp post is the mod's one prototype; the cycle itself only re-lights existing weather.
		LampPostRegistration.Register(registrator);
	}

	public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool gameWasLoaded) {
		// Shared, lazily-read view over config.json, injected into both cycle services.
		depBuilder.RegisterInstance(new NightModSettings(JsonConfig)).AsSelf();
		// The renderer is resolved on demand in Initialize (see below), never auto-created.
		depBuilder.RegisterDependency<NightCycleRenderer>().AsSelf();
	}

	public void EarlyInit(DependencyResolver resolver) {
	}

	public void Initialize(DependencyResolver resolver, bool gameWasLoaded) {
		// NightCycleManager auto-starts via [GlobalDependency]. The renderer depends on Unity's
		// LightController, so create it only when the game is actually rendering - in a headless
		// session LightController is not registered and the cycle simply runs without visuals.
		if (resolver.TryResolve<LightController>(out _)) {
			resolver.Resolve<NightCycleRenderer>();
		}

		// Supply the lamp post's procedural model. This must land in the asset database before the
		// first lamp post is rendered; it is skipped in a headless session, which has no AssetsDb.
		if (resolver.TryResolve<AssetsDb>(out AssetsDb assetsDb)) {
			LampPostAssets.Inject(assetsDb);
		}
	}

	public void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues) {
		// Saves made before the moon-phase setting existed have no stored value for it. Seed it to
		// true so those games keep the documented default (the phase follows the sun's angle).
		if (!savedValues.ContainsKey(NightModSettings.MoonPhaseFromSunKey)) {
			savedValues[NightModSettings.MoonPhaseFromSunKey] = true;
		}
	}

	public void Dispose() {
	}
}
