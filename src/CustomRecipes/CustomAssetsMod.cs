using Mafi;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using CustomAssets.Data.Mod;

namespace CustomAssets
{
	public sealed class CustomAssetsMod : DataOnlyMod {

		// Captured during RegisterPrototypes so Initialize can write the
		// runtime port catalog next to this mod's deployment folder.
		private string m_modBasePath;

		public CustomAssetsMod(ModManifest manifest) : base(manifest) {
			Log.Info($"{manifest.Id}: constructed");
		}

		public override void RegisterPrototypes(ProtoRegistrator registrator) {
			Log.Info($"{Manifest.Id}: registering prototypes");
			CustomAssetManager.Clear();
			PackRegistry.Clear();
			m_modBasePath = registrator.ActiveMod?.Manifest?.RootDirectoryPath ?? Manifest?.RootDirectoryPath;
			// Park the path on the registry so PackScaffolder can derive
			// the COI mods folder (parent of this directory) even on a save
			// that has no user packs registered yet â€” without it the
			// "create new pack" dialog would refuse to run on first use.
			PackRegistry.CoreModBasePath = m_modBasePath;

			// Stash this mod's own id + version on the API registry so
			// per-pack pin resolution can find our entry in each pack's
			// MandatoryDependencies array, AND so the "minimum required
			// pin" recommendation can compare against the framework's
			// actual current version. Captured here rather than in a
			// static field initializer because Manifest isn't available
			// at type-load time.
			ApiVersionRegistry.FrameworkModId  = Manifest?.Id ?? "CustomAssets";
			ApiVersionRegistry.FrameworkVersion = Manifest?.Version ?? default(VersionSlim);

			// Synchronous trace file so we can localize hangs even if Mafi.Log buffers.
			// Each Step() call flushes to disk before returning.
			DiagnosticTrace.Initialize(m_modBasePath);
			DiagnosticTrace.Step($"CustomAssetsMod.RegisterPrototypes: start (modBasePath={m_modBasePath})");

			try {
				DiagnosticTrace.Step("MachinePortsDumper.DumpToJson: invoking");
				MachinePortsDumper.DumpToJson(registrator.PrototypesDb, m_modBasePath);
				DiagnosticTrace.Step("MachinePortsDumper.DumpToJson: returned");
			} catch (System.Exception ex) {
				DiagnosticTrace.Step($"MachinePortsDumper.DumpToJson: THREW {ex.GetType().Name}: {ex.Message}");
				Log.Warning($"{Manifest?.Id ?? "CustomAssets"}: machine-ports dump failed: {ex.Message}");
			}

			DiagnosticTrace.Step("CustomAssetsMod.RegisterPrototypes: done");
		}
	}
}