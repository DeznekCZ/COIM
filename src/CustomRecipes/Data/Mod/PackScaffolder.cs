using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Mafi;

namespace CustomAssets.Data.Mod {

    /// <summary>
    /// Create a fresh CustomAssets pack folder on disk — manifest.json,
    /// CustomAssetPack.dll stub, Definitions/__init__.py, Assets/. The
    /// new pack will only become visible to the in-game editor after the
    /// modder reloads the save (Mafi loads mods once per session), so
    /// the caller should surface that requirement to the user.
    ///
    /// The scaffolder is intentionally conservative:
    ///   • refuses to overwrite an existing directory at the target path,
    ///   • refuses to create a pack with an id that collides with a
    ///     currently registered pack (would shadow it on reload),
    ///   • requires a source CustomAssetPack.dll be locatable on disk —
    ///     without the stub the manifest's primary_dlls entry would point
    ///     to a missing file and the pack would fail to load.
    ///
    /// Everything else (version, min_game_version, can_add_to_saved_game,
    /// authors) gets sensible defaults the modder can edit afterwards via
    /// the pack-deps dialog or directly in manifest.json.
    /// </summary>
    public static class PackScaffolder {

        public sealed class Result {
            public bool Success;
            /// Absolute path of the newly created pack folder, or null on
            /// failure. Always populated on success.
            public string PackRootPath;
            /// Human-readable error reason when Success is false. Phrased
            /// as a sentence so the UI can show it verbatim.
            public string Error;

            public static Result Ok(string path) {
                return new Result { Success = true, PackRootPath = path };
            }

            public static Result Fail(string error) {
                return new Result { Success = false, Error = error };
            }
        }

        // Mod ids end up as folder names AND Python module names downstream
        // (Definitions/__init__.py contents reference sibling .py files by
        // the pack id in some workflows), so we restrict to characters that
        // are safe in both. Underscores allowed; spaces, dots, dashes, and
        // other punctuation are not.
        private static readonly Regex s_validIdPattern =
            new Regex("^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.Compiled);

        /// <summary>Validate a candidate pack id. Returns null on success,
        /// or a sentence describing why the id is unacceptable.</summary>
        public static string ValidateId(string id) {
            if (string.IsNullOrWhiteSpace(id)) {
                return "Pack id must not be empty.";
            }
            if (!s_validIdPattern.IsMatch(id)) {
                return "Pack id must start with a letter and contain only letters, digits, and underscores.";
            }
            if (PackRegistry.TryGet(id, out _)) {
                return "A pack with id '" + id + "' is already loaded.";
            }
            return null;
        }

        /// <summary>Locate the mods folder the new pack should be created
        /// inside. Two strategies, tried in order:
        ///   1. <see cref="PackRegistry.CoreModBasePath"/> â€” the
        ///      CustomAssets mod's own directory, captured by
        ///      <see cref="CustomAssetsMod.RegisterPrototypes"/>. Its parent
        ///      is the COI mods folder. Works on a fresh save with zero
        ///      user packs because the editor itself ships inside a mod.
        ///   2. The parent of any currently-loaded pack's RootPath, same as
        ///      the legacy strategy. Kept as a fallback in case the
        ///      core-mod path wasn't captured (e.g. RegisterPrototypes
        ///      hasn't run yet â€” shouldn't happen at editor-open time,
        ///      but harmless safety net).
        /// Returns null only when both strategies fail.</summary>
        public static string TryFindModsRoot() {
            string corePath = PackRegistry.CoreModBasePath;
            if (!string.IsNullOrEmpty(corePath)) {
                string trimmed = corePath.TrimEnd(Path.DirectorySeparatorChar, '/');
                string modsDir = Path.GetDirectoryName(trimmed);
                if (!string.IsNullOrEmpty(modsDir) && Directory.Exists(modsDir)) {
                    return modsDir;
                }
            }
            foreach (LoadedPack pack in PackRegistry.Packs) {
                if (string.IsNullOrEmpty(pack.RootPath)) continue;
                string trimmed = pack.RootPath.TrimEnd(Path.DirectorySeparatorChar, '/');
                string modsDir = Path.GetDirectoryName(trimmed);
                if (!string.IsNullOrEmpty(modsDir) && Directory.Exists(modsDir)) {
                    return modsDir;
                }
            }
            return null;
        }

        /// <summary>Create the pack folder structure. <paramref name="modsRoot"/>
        /// must be an existing directory (the COI mods folder). The pack
        /// folder will be created at <c>&lt;modsRoot&gt;/&lt;packId&gt;/</c>.
        ///
        /// <paramref name="modDependencies"/> /
        /// <paramref name="optionalModDependencies"/> are written as the
        /// manifest's mod_dependencies / optional_mod_dependencies arrays.
        /// Null/empty falls back to the canonical default for required
        /// (a single CustomAssets pin) and an empty list for optional, so
        /// callers that don't yet collect deps keep the prior behaviour.
        ///
        /// <paramref name="minGameVersion"/> writes the manifest's
        /// min_game_version; null/empty keeps the canonical default.
        /// <paramref name="author"/>, when non-empty, becomes the single
        /// entry of the authors array. Empty leaves the array empty.</summary>
        public static Result Create(
                string modsRoot,
                string packId,
                string displayName,
                string descriptionShort,
                List<string> modDependencies = null,
                List<string> optionalModDependencies = null,
                string minGameVersion = null,
                string author = null) {
            if (string.IsNullOrEmpty(modsRoot) || !Directory.Exists(modsRoot)) {
                return Result.Fail("Mods folder not found - cannot determine where to write the new pack.");
            }
            string idError = ValidateId(packId);
            if (idError != null) {
                return Result.Fail(idError);
            }

            string packRoot = Path.Combine(modsRoot, packId);
            if (Directory.Exists(packRoot) || File.Exists(packRoot)) {
                return Result.Fail("A file or folder named '" + packId + "' already exists in the mods folder.");
            }

            try {
                Directory.CreateDirectory(packRoot);
                Directory.CreateDirectory(Path.Combine(packRoot, "Definitions"));
                Directory.CreateDirectory(Path.Combine(packRoot, "Assets"));

                writeManifest(packRoot, packId, displayName, descriptionShort,
                    modDependencies, optionalModDependencies, minGameVersion, author);
                writeInitPy(packRoot);

                // Copy the shared CustomAssetPack.dll stub. Without this
                // the manifest's primary_dlls entry would point to a
                // missing file and the pack would fail to load with an
                // unhelpful "DLL not found" on the next session.
                string copied = LegacyMigrator.CopyPackStubInto(packRoot);
                if (string.IsNullOrEmpty(copied)) {
                    // We created the folder structure but couldn't drop
                    // the stub. Surface this so the modder can copy the
                    // DLL in manually rather than discover the broken
                    // pack on next launch.
                    return Result.Fail("Pack folder created at '" + packRoot
                        + "' but CustomAssetPack.dll could not be located to copy."
                        + " Copy it from another loaded pack and the new pack will load on next session.");
                }
                return Result.Ok(packRoot);
            } catch (Exception ex) {
                Log.Warning("PackScaffolder: create failed - " + ex.Message);
                // Best-effort cleanup so we don't leave a half-created
                // pack folder behind that would block a second attempt
                // with the same id.
                try {
                    if (Directory.Exists(packRoot)) {
                        Directory.Delete(packRoot, recursive: true);
                    }
                } catch (Exception cleanupEx) {
                    Log.Warning("PackScaffolder: cleanup after failed create also failed - " + cleanupEx.Message);
                }
                return Result.Fail("Could not create pack folder: " + ex.Message);
            }
        }

        private static void writeManifest(string packRoot, string packId,
                string displayName, string descriptionShort,
                List<string> modDependencies,
                List<string> optionalModDependencies,
                string minGameVersion,
                string author) {
            // Only fields whose value carries meaning end up in the file:
            //   * id / display_name / primary_dlls / primary_mod_class_name
            //     are always required (the pack can't load without them),
            //     so they're written unconditionally.
            //   * Booleans (can_add_to_saved_game, can_remove_from_saved_game,
            //     non_locking_dll_load) all default to FALSE in Mafi's
            //     ModManifest ctor; our preferred values flip them to TRUE,
            //     which IS the meaning we want to surface, so we write them.
            //   * The remaining string / array fields are written only when
            //     the modder supplied a non-empty value — empty strings and
            //     empty arrays carry no info beyond Mafi's defaults so we
            //     drop them to keep the manifest minimal.
            //   * min_game_version: blank input falls back to the canonical
            //     "0.8.4" so we still pin against a known-good baseline.
            //     The version field stays "0.0.1" because Mafi requires a
            //     parseable version to load.
            Dictionary<string, object> manifest = new Dictionary<string, object> {
                { "id",                       packId },
                { "display_name",             string.IsNullOrEmpty(displayName) ? packId : displayName },
                { "version",                  "0.0.1" },
                { "can_add_to_saved_game",    true },
                { "can_remove_from_saved_game", true },
                { "non_locking_dll_load",     true },
                { "min_game_version",         string.IsNullOrWhiteSpace(minGameVersion) ? "0.8.4" : minGameVersion.Trim() },
                { "primary_dlls",             new List<object> { "CustomAssetPack.dll" } },
                { "primary_mod_class_name",   "CustomAssetPack" }
            };

            if (!string.IsNullOrWhiteSpace(descriptionShort)) {
                manifest["description_short"] = descriptionShort.Trim();
            }
            if (!string.IsNullOrWhiteSpace(author)) {
                manifest["authors"] = new List<object> { author.Trim() };
            }

            List<object> modDeps = trimStringList(modDependencies);
            if (modDeps.Count > 0) {
                manifest["mod_dependencies"] = modDeps;
            }
            List<object> optionalDeps = trimStringList(optionalModDependencies);
            if (optionalDeps.Count > 0) {
                manifest["optional_mod_dependencies"] = optionalDeps;
            }

            string path = Path.Combine(packRoot, "manifest.json");
            File.WriteAllText(path, MiniJsonWriter.Write(manifest),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        // Strip null/whitespace entries and trim survivors so a half-filled
        // UI list doesn't write blank specs into the manifest. Returns an
        // empty list (not null) so callers can length-check uniformly.
        private static List<object> trimStringList(List<string> input) {
            List<object> result = new List<object>();
            if (input == null) return result;
            foreach (string s in input) {
                if (string.IsNullOrWhiteSpace(s)) continue;
                result.Add(s.Trim());
            }
            return result;
        }

        private static void writeInitPy(string packRoot) {
            // Definitions/__init__.py is the new-pattern entry point COI's
            // Python loader executes first. Leave it as a commented
            // placeholder so the modder sees exactly where new imports go
            // without having to remember the pattern.
            string content =
                "# Definitions/__init__.py is loaded first by the CustomAssets framework.\n" +
                "# Add an `import <module>` line for each .py file you create in this folder,\n" +
                "# in the order they should be loaded.\n" +
                "#\n" +
                "# Example:\n" +
                "#   import recipes\n" +
                "#   import products\n";
            string path = Path.Combine(packRoot, "Definitions", "__init__.py");
            File.WriteAllText(path, content,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
