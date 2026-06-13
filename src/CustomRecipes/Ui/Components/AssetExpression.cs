namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Round-trip helpers between an "asset path-ish" value and the raw Python
    /// expression stored on model fields like <c>MaterialLooseDef.AlbedoExpression</c>.
    ///
    /// Those raw-expression fields hold whatever the modder wrote in source:
    ///   • <c>"Assets/MyPack/foo.png"</c> — a quoted string literal path,
    ///   • <c>Assets.Base.Foo_png</c> — a dotted typed-ref to a const,
    ///   • <c>myTex</c> — a bare identifier referencing a same-file
    ///     variable (e.g. <c>myTex = add_texture(...)</c>),
    ///   • <c>Tex.foo</c> / a list / arbitrary code — anything not in the
    ///     three shapes above falls through unchanged (the field stays a
    ///     TextField for manual edits; the picker can't help there).
    ///
    /// The picker only knows about "values" (bare paths, dotted refs, or
    /// variable names), so <see cref="Parse"/> strips the quotes off a
    /// quoted literal and <see cref="Render"/> puts them back when emitting.
    /// Bare identifiers and dotted refs emit WITHOUT quotes so Python
    /// resolves them at pack-load time. Anything else (lists, calls)
    /// round-trips verbatim.
    /// </summary>
    public static class AssetExpression {

        /// Convert a raw Python expression string into a picker-friendly value.
        /// • <c>"path/foo.png"</c> → <c>path/foo.png</c>
        /// • <c>Assets.Base.X</c> → <c>Assets.Base.X</c>
        /// • <c>myVariable</c> → <c>myVariable</c>
        /// • empty / null → null
        /// • anything else (list, call, etc.) → returns null so the picker
        ///   shows "(no asset)" but doesn't corrupt the raw expression.
        public static string Parse(string rawExpr) {
            if (string.IsNullOrWhiteSpace(rawExpr)) return null;
            string s = rawExpr.Trim();
            if (s.Length >= 2
                    && (s[0] == '"' || s[0] == '\'')
                    && s[s.Length - 1] == s[0]) {
                return s.Substring(1, s.Length - 2);
            }
            if (IsDottedRef(s)) return s;
            if (IsBareIdentifier(s)) return s;
            return null;
        }

        /// Convert a picker-emitted value back into a raw Python expression.
        /// • null → null (caller writes null to clear the field)
        /// • dotted ref → emit bare so Python resolves the const
        /// • bare identifier → emit bare so Python resolves the variable
        ///   binding (e.g. <c>prefab = my_unit_prefab</c> instead of
        ///   <c>prefab = "my_unit_prefab"</c> — the latter would be looked
        ///   up as an asset path and fail). Distinguished from a string
        ///   path by structure: paths contain '/' / '.' (an extension) /
        ///   non-identifier chars, identifiers don't.
        /// • anything else → wrap in double quotes as a string literal
        public static string Render(string value) {
            if (string.IsNullOrEmpty(value)) return null;
            if (IsDottedRef(value)) return value;
            if (IsBareIdentifier(value)) return value;
            return "\"" + value + "\"";
        }

        public static bool IsDottedRef(string s) {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s) {
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_') return false;
            }
            return s.IndexOf('.') >= 0;
        }

        /// Bare Python identifier — letters / digits / underscores, with a
        /// leading letter or underscore. Used to spot variable references
        /// from the picker's "Pack variables" section so they emit bare
        /// instead of quoted. Single-token strings without a dot or slash
        /// are unambiguous as identifiers (asset paths in COI always
        /// contain either a slash or an extension dot, never just a bare
        /// word).
        public static bool IsBareIdentifier(string s) {
            if (string.IsNullOrEmpty(s)) return false;
            char first = s[0];
            if (!char.IsLetter(first) && first != '_') return false;
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                if (!char.IsLetterOrDigit(c) && c != '_') return false;
            }
            return true;
        }
    }
}
