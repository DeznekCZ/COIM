using System.Collections.Generic;

namespace CustomAssets.Editor.Model {

    /// One ingredient or output. Mirrors the `Product(product, quantity, port="*")`
    /// constructor from the CustomAssets Python API. IDs are kept as strings —
    /// modders may reference products by Ids.Products.X (typed) or "Product_X"
    /// (literal), but at the editor's level they're all opaque identifiers.
    public sealed class ProductRef {
        public string ProductId;
        public int Quantity;
        public string Port;       // null or "" means default ("*")

        public ProductRef() { }

        public ProductRef(string productId, int quantity, string port = null) {
            ProductId = productId;
            Quantity = quantity;
            Port = port;
        }
    }

    /// One recipe — mirrors the `build_recipe(...)` API surface in
    /// src/Recipes/CustomAssets/__init__.py. Optional fields use null to mean
    /// "argument was omitted in the source"; the emitter must reproduce this
    /// distinction (omitted vs explicit-default) faithfully so the file stays stable.
    public sealed class RecipeDef {
        // Required arguments.
        public string RecipeId;
        public string Name;
        public string Description;
        public string MachineId;

        // Optional arguments. Null = not present in source.
        public string ResearchId;
        public int? DurationSeconds;
        public List<ProductRef> Ingredients = new List<ProductRef>();
        public List<ProductRef> Products    = new List<ProductRef>();
        public int? PowerPercent;

        // Free-form modder notes captured from `#` comment lines immediately
        // preceding the build_recipe(...) call. Loader walks back from
        // SourceStartLine collecting consecutive comment lines (a blank line
        // breaks the block) and joins them with '\n'. Emitter reverses the
        // process. Editing this field updates the comment block on save.
        // Null/empty = no comment.
        public string Comment;

        // Source-location bookkeeping. Populated by PackLoader from
        // EvaluateStatement.StartLine/EndLine; used by PackEmitter to splice the
        // canonical re-emission of this call back into its original file by line
        // range, leaving everything else (comments, imports, helper code) untouched.
        // SourceFile is the absolute path. Lines are 1-based, inclusive.
        // For recipes ADDED in the editor (no source yet), SourceFile is null and
        // both line numbers are 0; the emitter appends to the chosen file.
        public string SourceFile;
        public int SourceStartLine;
        public int SourceEndLine;
    }

    /// One pack worth of recipes the editor has loaded. Keyed off PackRegistry's
    /// LoadedPack — same ModId, same RootPath. Recipes are flat across the pack's
    /// files; SourceFile on each RecipeDef preserves the file the user originally
    /// put it in so saving doesn't shuffle definitions between files.
    public sealed class PackModel {
        public string ModId;
        public string RootPath;
        public List<RecipeDef> Recipes = new List<RecipeDef>();

        public PackModel(string modId, string rootPath) {
            ModId = modId;
            RootPath = rootPath;
        }
    }
}
