using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Localization;

namespace CustomAssets.Data.Translate {

    /// Per-pack translation loader.
    ///
    /// The TranslationsPanel writes each pack's translation file to
    ///   &lt;pack&gt;/Translations/&lt;lang&gt;.json
    /// as a flat dict of dialog-shape keys:
    ///   recipe.&lt;RecipeId&gt;.name             -&gt; localized recipe display name
    ///   recipe.&lt;RecipeId&gt;.description      -&gt; localized recipe description
    ///   &lt;Kind&gt;.&lt;ProtoId&gt;.name             -&gt; localized name for non-recipe defs
    ///   &lt;Kind&gt;.&lt;ProtoId&gt;.description      -&gt; localized description for non-recipe defs
    /// (Kind comes from DefBase.Kind, e.g. "ProductLoose", "Research"…)
    ///
    /// The Mafi framework looks up LocStr translations by ID using
    /// <c>LocalizationManager.s_data["&lt;protoId&gt;__name"]</c> /
    /// <c>"__desc"</c>. So the loader transforms each dialog key into the
    /// matching Mafi ID and splices it into <c>s_data</c> via reflection.
    ///
    /// Timing: this must run BEFORE the pack's Python .py files execute
    /// (build_recipe / build_research / build_product_* call Loc.Str
    /// during proto registration, which snapshots the current s_data into
    /// the returned LocStr's TranslatedString field). The right hook is
    /// <see cref="CustomAssetRegistrator.RegisterData"/>, right after the
    /// pack's m_modBasePath/m_modId are set and before the __init__.py
    /// load — see the call site there.
    ///
    /// English (default culture): <c>LocalizationManager.s_data</c> is
    /// null; the loader is a no-op and English literals in the .py files
    /// are used as-is, which is the desired behavior.
    public static class ModTranslations {

        /// <see cref="Editor.Model.RenameResearchDef.Kind"/> — the dialog-key
        /// prefix a `rename_research` row carries in the Translations panel.
        /// Named here because this file owns the key → loc-id mapping.
        public const string RenameResearchKind = "rename-research";

        // Loc-id namespace for every string a pack SUBSTITUTES for one the
        // game already registered. Renames cannot reuse the target's own
        // "<protoId>__name" id: that id is already in LocalizationManager's
        // en-US dict (the game registered it when it built the proto), so
        // re-registering it logs a duplicate-ID error, and splicing a
        // translation into it would arrive far too late — the vanilla proto
        // froze its LocStr at ITS registration time, long before any mod ran.
        private const string RenamePrefix = "CustomAssetsRename_";

        // Reflection handle to LocalizationManager.s_data. It's a private
        // static Dict<string, LocData>; the indexer is settable, so we just
        // cast and assign once we have the reference.
        private static readonly FieldInfo s_dataField =
            typeof(LocalizationManager).GetField(
                "s_data", BindingFlags.NonPublic | BindingFlags.Static);

        /// Load <paramref name="packRootPath"/>/Translations/&lt;currentLang&gt;.json
        /// (if present) and splice each entry into LocalizationManager.s_data
        /// under the matching Mafi LocStr ID. Returns the number of entries spliced.
        ///
        /// Safe to call when the language file does not exist, when the dict
        /// fails to parse, or when running under English (s_data is null in
        /// that case). Errors are logged, never thrown — a broken translation
        /// file must not prevent the pack from loading.
        public static int LoadForPack(string packId, string packRootPath) {
            if (string.IsNullOrEmpty(packRootPath)) return 0;

            string fileName = LocalizationManager.CurrentLangInfo.FileName;
            string langPath = Path.Combine(packRootPath, "Translations", fileName);
            if (!File.Exists(langPath)) {
                Log.Info($"ModTranslations[{packId}]: no '{fileName}' under 'Translations/'; using English defaults.");
                return 0;
            }

            // s_data is null when running under English (the default culture
            // doesn't load any translation file). Nothing to splice — Mafi
            // will fall back to the English literal we pass to Loc.Str(...).
            if (!(s_dataField?.GetValue(null) is Dict<string, LocalizationManager.LocData> sData)) {
                Log.Info($"ModTranslations[{packId}]: current language has no s_data " +
                         "(likely English); nothing to splice.");
                return 0;
            }

            object parsed;
            try {
                parsed = MiniJson.Parse(File.ReadAllText(langPath));
            } catch (Exception ex) {
                Log.Warning($"ModTranslations[{packId}]: failed to parse '{langPath}': {ex.Message}");
                return 0;
            }
            if (!(parsed is Dictionary<string, object> dict)) {
                Log.Warning($"ModTranslations[{packId}]: '{langPath}' did not parse to a JSON object.");
                return 0;
            }

            int spliced = 0;
            int skipped = 0;
            foreach (KeyValuePair<string, object> kvp in dict) {
                if (!(kvp.Value is string translated)) { skipped++; continue; }
                if (string.IsNullOrEmpty(translated))  { skipped++; continue; }

                string locId = MapPackKeyToLocId(kvp.Key, packId);
                if (locId == null) { skipped++; continue; }

                sData[locId] = new LocalizationManager.LocData(ImmutableArray.Create(translated));
                spliced++;
            }
            Log.Info($"ModTranslations[{packId}]: spliced {spliced} translation(s) from '{fileName}' " +
                     (skipped > 0 ? $"({skipped} skipped) " : "") +
                     $"into LocalizationManager.s_data.");
            return spliced;
        }

        /// Map a dialog/per-pack key to the Mafi LocStr ID the framework
        /// will look up when registering a prototype.
        ///   "recipe.MyRecipe.name"        -&gt; "MyRecipe__name"
        ///   "recipe.MyRecipe.description" -&gt; "MyRecipe__desc"
        ///   "ProductLoose.MyOre.name"     -&gt; "MyOre__name"
        ///   "Research.MyTech.description" -&gt; "MyTech__desc"
        /// Anything that doesn't fit the &lt;prefix&gt;.&lt;id&gt;.&lt;field&gt;
        /// shape, or has an unknown field, returns null and is silently
        /// skipped (callers count these as "skipped").
        ///
        /// Implementation note: we split on the FIRST and LAST dot so that
        /// IDs containing underscores or other characters survive intact.
        /// IDs containing literal dots are unsupported (none in COI's proto
        /// naming convention).
        ///
        /// One prefix is special: <c>rename-research</c> keys do NOT map to the
        /// target's own loc id — see <see cref="RenameLocId"/> for why — so
        /// <paramref name="packId"/> is needed to build the substitute id. Pass
        /// null when no pack context is available; rename keys then map to null
        /// (i.e. are skipped) rather than silently overwriting a vanilla entry.
        public static string MapPackKeyToLocId(string packKey, string packId = null) {
            if (string.IsNullOrEmpty(packKey)) return null;
            int firstDot = packKey.IndexOf('.');
            int lastDot  = packKey.LastIndexOf('.');
            if (firstDot < 0 || lastDot <= firstDot) return null;

            string prefix = packKey.Substring(0, firstDot);
            string id     = packKey.Substring(firstDot + 1, lastDot - firstDot - 1);
            string field  = packKey.Substring(lastDot + 1);
            if (string.IsNullOrEmpty(id)) return null;

            bool isDescription;
            switch (field) {
                case "name":        isDescription = false; break;
                case "description": isDescription = true;  break;
                default: return null;
            }

            if (prefix == RenameResearchKind) {
                if (string.IsNullOrEmpty(packId)) return null;
                return RenameLocId(packId, id, isDescription);
            }
            return id + (isDescription ? Loc.DESC_SUFFIX : Loc.NAME_SUFFIX);
        }

        /// Loc id under which a `rename_research` call registers its
        /// replacement text. Both ends of the pipeline call this — the runtime
        /// when it builds the LocStr, this loader when it splices the
        /// translated value in — so the two can never drift apart.
        ///
        /// The pack id is part of the id so two packs renaming the SAME node
        /// each keep their own translation (and neither trips
        /// LocalizationManager's duplicate-ID check). Whichever pack loads last
        /// wins the node's title, and it wins with its own translated string.
        public static string RenameLocId(string packId, string protoId, bool isDescription) {
            return RenamePrefix + sanitizeIdPart(packId) + "_" + sanitizeIdPart(protoId)
                   + (isDescription ? Loc.DESC_SUFFIX : Loc.NAME_SUFFIX);
        }

        // Loc ids are plain dictionary keys, but mod ids may carry spaces,
        // dots or dashes that would make the resulting id impossible to read
        // in a translation file. Fold anything outside [A-Za-z0-9_] to '_'.
        private static string sanitizeIdPart(string raw) {
            if (string.IsNullOrEmpty(raw)) return "";
            char[] chars = raw.ToCharArray();
            for (int i = 0; i < chars.Length; i++) {
                char c = chars[i];
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
                          || (c >= '0' && c <= '9') || c == '_';
                if (!ok)
                {
                    chars[i] = '_';
                }
            }
            return new string(chars);
        }
    }
}
