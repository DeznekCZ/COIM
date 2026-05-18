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
			m_modBasePath = registrator.ActiveMod?.Manifest?.RootDirectoryPath ?? Manifest?.RootDirectoryPath;

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