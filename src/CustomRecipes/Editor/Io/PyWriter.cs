using System.Text;

namespace CustomAssets.Editor.Io {

    /// <summary>
    /// Builds Python source with STRUCTURAL indentation instead of hand-written
    /// spaces, so a renderer never spells out <c>"    "</c> and can't drift when
    /// its statement ends up nested more deeply than its author imagined.
    ///
    /// Every writer in a chain shares ONE <see cref="StringBuilder"/>; what
    /// differs is the <c>indent</c> prefix each carries. <see cref="Indent"/>
    /// hands back a writer one level deeper over that same buffer:
    ///
    /// <code>
    /// StringBuilder sb = new StringBuilder();
    /// PyWriter w = new PyWriter(sb);
    /// w.Append("with build_recipe(");
    /// w.Indent().Append("\nrecipeId = \"X\"");
    /// w.Append("\n):");
    /// w.Indent().Append("\nbind_recipe(...)");
    /// </code>
    ///
    /// Nothing is ever closed. A nested writer is just a value you stop using —
    /// the parent still carries its own indent — so there is no open/close
    /// pairing to get wrong and no fold-back step to forget.
    ///
    /// The single rule is: <b>every newline this writer emits is followed by its
    /// indent</b>. The first line of an <see cref="Append"/> therefore continues
    /// wherever the buffer already sits and gets no indent, which is what lets a
    /// line be assembled from several fragments the way the emitter already does
    /// (<c>",\n"</c> + a list + a closer).
    ///
    /// Depth here is RELATIVE to the statement, not absolute in the file: a
    /// rendered statement always starts at column 0 and the splice layer shifts
    /// the whole block to wherever it sits (see PackEmitter.rewriteFile, which
    /// re-applies the original line's leading whitespace). Keeping it relative is
    /// what lets the same rendered text be spliced at top level, inside an `if`,
    /// or inside a `with` nested two levels down without re-rendering it.
    /// </summary>
    internal readonly struct PyWriter {

        /// One indentation step. Python-side files use 4 spaces throughout.
        public const string IndentUnit = "    ";

        private readonly StringBuilder m_sb;
        private readonly string m_indent;

        public PyWriter(StringBuilder sb, string indent = "") {
            m_sb = sb;
            m_indent = indent ?? "";
        }

        /// A writer over a fresh buffer, at column 0.
        public static PyWriter Create() {
            return new PyWriter(new StringBuilder());
        }

        /// The shared buffer, for the rare caller that still needs to poke at it
        /// directly (length checks, trailing-comma trimming).
        public StringBuilder Buffer => m_sb;

        /// This writer's own prefix — what it writes after each newline.
        public string CurrentIndent => m_indent;

        /// Append <paramref name="text"/>. Its FIRST line continues wherever the
        /// buffer already is and gets no indent; every newline INSIDE it is
        /// followed by this writer's indent.
        public PyWriter Append(string text) {
            if (string.IsNullOrEmpty(text)) return this;
            string[] lines = text.Split('\n');
            m_sb.Append(lines[0]);
            for (int i = 1; i < lines.Length; i++) {
                m_sb.Append('\n').Append(m_indent).Append(lines[i]);
            }
            return this;
        }

        /// As <see cref="Append"/>, then end the line — so whatever is appended
        /// next starts at this writer's indent.
        public PyWriter AppendLine(string text = "") {
            Append(text);
            m_sb.Append('\n').Append(m_indent);
            return this;
        }

        /// A writer one level deeper, over the SAME buffer.
        public PyWriter Indent() {
            return new PyWriter(m_sb, m_indent + IndentUnit);
        }

        /// The rendered statement: trailing whitespace stripped from every line
        /// and no trailing newline, which is the shape every renderer returns
        /// today. Per-line trimming is what keeps a blank line blank — the indent
        /// is written after EVERY newline, including the ones that start an empty
        /// line, and Python never gives trailing whitespace meaning.
        public override string ToString() {
            if (m_sb == null) return "";
            string[] lines = m_sb.ToString().Split('\n');
            for (int i = 0; i < lines.Length; i++) {
                lines[i] = lines[i].TrimEnd();
            }
            return string.Join("\n", lines).TrimEnd('\n');
        }
    }
}
