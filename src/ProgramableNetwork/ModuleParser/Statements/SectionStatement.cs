using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    // PLC-PY top-level section marker — a `init:` or `main:` block in the
    // player's script.  Parsed by the lexer as a structured statement so the
    // section split lives in the same indent-stack-aware code path that
    // handles `def`, `class`, `if`, etc., instead of a text-level pre-pass
    // that can mis-detect headers when the editor mangles whitespace.
    //
    // Doesn't implement Execute — sections aren't dispatched per-statement
    // by the runtime.  PlcPy.Action walks the top-level block, pulls the
    // SectionStatements out by Name into separate init / main blocks, and
    // treats the remaining statements as the preamble.  If a SectionStatement
    // somehow ends up inside a runtime-executed Block (which shouldn't
    // happen — the lexer only emits them at top level), Execute throws so
    // the bug surfaces immediately rather than silently no-op'ing.
    internal class SectionStatement : IStatement
    {
        public readonly string Name;
        public readonly Block Body;

        public SectionStatement(string name, Block body)
        {
            Name = name;
            Body = body;
        }

        public void Execute(IDictionary<string, object> context)
        {
            throw new PythonRuntimeException(
                $"PLC-PY section `{Name}:` reached the per-tick executor — "
                + "PlcPy.Action should have peeled it out of the preamble.");
        }
    }
}
