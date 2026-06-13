using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using Mafi;
using PythonAPI;
using PythonAPI.Statements;
using CustomAssets.Python;

namespace CustomAssets.Editor.Io {

    /// One round-trip check: load a pack -> render each recipe -> re-parse the rendered
    /// text -> compare the second load against the first. If load2 produces RecipeDefs
    /// equal to load1's, the renderer is at least round-trip-stable. Doesn't prove the
    /// renderer matches the original file byte-for-byte (it won't — formatting differs),
    /// only that semantic content survives a save/reload cycle.
    ///
    /// Run from the editor's "Verify Round-Trip" button (todo: add it). Logs human-
    /// readable findings via Mafi's Log; the editor UI can also surface a pass/fail
    /// banner.
    public static class RoundTripTester {

        public sealed class Report {
            public int RecipesChecked;
            public int Passed;
            public int Failed;
            public List<string> Messages = new List<string>();
            public bool OverallPass => Failed == 0;
        }

        /// Run round-trip checks against every pack currently in PackRegistry.
        public static Report RunOnRegisteredPacks() {
            Report report = new Report();
            // Layout codec round-trip self-test runs first so a regression in
            // the tile-grid serialiser surfaces alongside the recipe checks.
            string layoutFail = LayoutCodec.SelfTest();
            if (layoutFail != null) {
                report.Failed++;
                report.Messages.Add("LayoutCodec.SelfTest FAILED: " + layoutFail);
            } else {
                report.Messages.Add("LayoutCodec.SelfTest passed.");
            }
            foreach (LoadedPack pack in PackRegistry.Packs) {
                runOnPack(pack, report);
            }
            log(report);
            return report;
        }

        /// Run round-trip checks against a specific pack.
        public static Report RunOnPack(LoadedPack pack) {
            Report report = new Report();
            runOnPack(pack, report);
            log(report);
            return report;
        }

        private static void runOnPack(LoadedPack pack, Report report) {
            PackModel firstLoad = PackLoader.Load(pack);

            foreach (RecipeDef original in firstLoad.Recipes) {
                report.RecipesChecked++;

                string rendered = PackEmitter.RenderRecipe(original);
                RecipeDef reparsed = parseSingleRecipeFromText(rendered, original.SourceFile);

                if (reparsed == null) {
                    report.Failed++;
                    report.Messages.Add(
                        $"[{pack.ModId}] {original.RecipeId}: rendered output did not " +
                        "re-parse as a build_recipe call. Output was:\n" + rendered);
                    continue;
                }

                List<string> diffs = compareRecipes(original, reparsed);
                if (diffs.Count == 0) {
                    report.Passed++;
                } else {
                    report.Failed++;
                    report.Messages.Add(
                        $"[{pack.ModId}] {original.RecipeId}: " +
                        $"{diffs.Count} difference(s) after round-trip:\n  " +
                        string.Join("\n  ", diffs));
                }
            }
        }

        // Tokenise + parse a single rendered build_recipe string. Reuses the same
        // Tokenizer / Lexer the mod uses at load time, so any tokenizer-level quirks
        // in our output get caught immediately.
        private static RecipeDef parseSingleRecipeFromText(string text, string sourceFileHint) {
            FileInfo fileInfo = new FileInfo(sourceFileHint ?? "<round-trip>");
            Token[] tokens = Tokenizer.ParseLines(fileInfo, text.Split('\n'));
            Block block = Lexer.Parse(tokens);

            // We expect exactly one EvaluateStatement at the top level.
            LoadedFile synthetic = new LoadedFile(fileInfo.FullName, block);
            LoadedPack syntheticPack = new LoadedPack("<round-trip>", "");
            syntheticPack.Files.Add(synthetic);
            PackModel m = PackLoader.Load(syntheticPack);
            return m.Recipes.Count() == 1 ? m.Recipes.First() : null;
        }

        private static List<string> compareRecipes(RecipeDef a, RecipeDef b) {
            List<string> diffs = new List<string>();
            if (a.RecipeId    != b.RecipeId)    diffs.Add($"RecipeId: '{a.RecipeId}' vs '{b.RecipeId}'");
            if (a.Name        != b.Name)        diffs.Add($"Name: '{a.Name}' vs '{b.Name}'");
            if (a.Description != b.Description) diffs.Add($"Description: '{a.Description}' vs '{b.Description}'");
            if (a.MachineId   != b.MachineId)   diffs.Add($"MachineId: '{a.MachineId}' vs '{b.MachineId}'");
            if (a.ResearchId  != b.ResearchId)  diffs.Add($"ResearchId: '{a.ResearchId}' vs '{b.ResearchId}'");
            if (a.DurationSeconds != b.DurationSeconds)
                diffs.Add($"DurationSeconds: {a.DurationSeconds} vs {b.DurationSeconds}");
            if (a.PowerPercent != b.PowerPercent)
                diffs.Add($"PowerPercent: {a.PowerPercent} vs {b.PowerPercent}");
            compareProductLists(a.Ingredients, b.Ingredients, "Ingredients", diffs);
            compareProductLists(a.Products,    b.Products,    "Products",    diffs);
            return diffs;
        }

        private static void compareProductLists(List<ProductRef> a, List<ProductRef> b,
                                                string label, List<string> diffs) {
            int aCount = a?.Count ?? 0;
            int bCount = b?.Count ?? 0;
            if (aCount != bCount) {
                diffs.Add($"{label}.Count: {aCount} vs {bCount}");
                return;
            }
            for (int i = 0; i < aCount; i++) {
                ProductRef ai = a[i], bi = b[i];
                if (ai.ProductId != bi.ProductId)
                    diffs.Add($"{label}[{i}].ProductId: '{ai.ProductId}' vs '{bi.ProductId}'");
                if (ai.Quantity != bi.Quantity)
                    diffs.Add($"{label}[{i}].Quantity: {ai.Quantity} vs {bi.Quantity}");
                if (normPort(ai.Port) != normPort(bi.Port))
                    diffs.Add($"{label}[{i}].Port: '{ai.Port}' vs '{bi.Port}'");
            }
        }

        // Port "*" and null are equivalent (the API defaults to "*"); normalise for diff.
        private static string normPort(string p) =>
            string.IsNullOrEmpty(p) ? "*" : p;

        private static void log(Report r) {
            Log.Info($"RoundTripTester: {r.RecipesChecked} recipe(s) checked, " +
                     $"{r.Passed} passed, {r.Failed} failed.");
            foreach (string msg in r.Messages) {
                if (r.Failed == 0) Log.Info("  " + msg);
                else Log.Warning("  " + msg);
            }
        }
    }
}
