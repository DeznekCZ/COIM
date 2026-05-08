using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomAssets.StubBuilder
{
    internal sealed class ScannedIds
    {
        public SortedSet<string> Recipes { get; } = new SortedSet<string>();
        public SortedSet<string> Research { get; } = new SortedSet<string>();
        public SortedSet<string> Products { get; } = new SortedSet<string>();
        public SortedSet<string> Machines { get; } = new SortedSet<string>();
        public SortedSet<string> ToolbarCategories { get; } = new SortedSet<string>();

        public bool IsEmpty =>
            Recipes.Count == 0 &&
            Research.Count == 0 &&
            Products.Count == 0 &&
            Machines.Count == 0 &&
            ToolbarCategories.Count == 0;
    }

    internal static class IdScanner
    {
        // Keyword-arg patterns:  recipeId = "X" / recipeId="X"
        private static readonly Regex RecipeIdRegex =
            new Regex(@"\brecipeId\s*=\s*[""']([A-Za-z0-9_]+)[""']", RegexOptions.Compiled);
        private static readonly Regex ResearchIdRegex =
            new Regex(@"\bresearchId\s*=\s*[""']([A-Za-z0-9_]+)[""']", RegexOptions.Compiled);
        private static readonly Regex ProductIdRegex =
            new Regex(@"\bproductId\s*=\s*[""']([A-Za-z0-9_]+)[""']", RegexOptions.Compiled);
        private static readonly Regex MachineIdRegex =
            new Regex(@"\bmachineId\s*=\s*[""']([A-Za-z0-9_]+)[""']", RegexOptions.Compiled);
        private static readonly Regex CategoryIdRegex =
            new Regex(@"\bcategoryId\s*=\s*[""']([A-Za-z0-9_]+)[""']", RegexOptions.Compiled);

        // Implicit product references used positionally: Product("Product_X", ...)
        // and add_unlock_product(..., "Product_X")
        private static readonly Regex ProductLiteralRegex =
            new Regex(@"[""'](Product_[A-Za-z0-9_]+)[""']", RegexOptions.Compiled);
        private static readonly Regex MachineLiteralRegex =
            new Regex(@"[""'](Custom(?:Machine|Building|Entity)_[A-Za-z0-9_]+)[""']", RegexOptions.Compiled);

        public static ScannedIds ScanDirectory(string definitionsDir)
        {
            var result = new ScannedIds();
            if (!Directory.Exists(definitionsDir)) return result;

            foreach (var file in Directory.EnumerateFiles(definitionsDir, "*.py", SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(file);
                CollectMatches(RecipeIdRegex, text, result.Recipes);
                CollectMatches(ResearchIdRegex, text, result.Research);
                CollectMatches(ProductIdRegex, text, result.Products);
                CollectMatches(MachineIdRegex, text, result.Machines);
                CollectMatches(CategoryIdRegex, text, result.ToolbarCategories);
                CollectMatches(ProductLiteralRegex, text, result.Products);
                CollectMatches(MachineLiteralRegex, text, result.Machines);
            }
            return result;
        }

        private static void CollectMatches(Regex regex, string text, SortedSet<string> sink)
        {
            foreach (Match m in regex.Matches(text))
                if (m.Groups.Count > 1)
                    sink.Add(m.Groups[1].Value);
        }
    }
}
