using System.Collections.Generic;
using System.IO;
using Mafi;
using PythonAPI;
using PythonAPI.Arguments;
using PythonAPI.Expressions;
using PythonAPI.Statements;

namespace CustomAssets.Data.Mod {

    /// <summary>
    /// Walks a pack's parsed Python AST and computes which CustomAssets
    /// framework version it actually needs — i.e. the maximum
    /// <see cref="ApiVersionRegistry"/> <c>Since</c> across every call /
    /// named argument it uses. Compared to the pack's own
    /// <c>mod_dependencies</c> pin, this surfaces two things:
    ///
    ///   1. The single "minimum required" version recommendation the
    ///      modder should pin against — actionable in one sentence.
    ///   2. Per-call detail (which arg pulled the version up) so the
    ///      modder can either drop the late arg or accept the bump.
    ///
    /// The analyzer is read-only — never throws, never modifies state.
    /// Bad/unparseable args are simply skipped; the registry doesn't
    /// pretend to know things it doesn't know.
    /// </summary>
    public static class PackVersionAnalyzer {

        /// <summary>
        /// Per-call/arg datum explaining why the required version went
        /// up. Stored as a flat list so the warning emitter can dedupe
        /// and order them.
        /// </summary>
        public sealed class UsageNote {
            /// <summary>The Python file the call appears in.</summary>
            public string FileName;
            /// <summary>Source line of the call. 0 when the AST doesn't
            /// carry one for this statement kind.</summary>
            public int Line;
            /// <summary>The call's name (e.g. "add_unit_prefab").</summary>
            public string Call;
            /// <summary>Named arg that bumped the required version, or
            /// empty when it's the call itself that's new.</summary>
            public string ArgName;
            /// <summary>The version this call/arg requires.</summary>
            public VersionSlim Required;
        }

        public sealed class Report {
            /// <summary>The maximum <c>Since</c> across every recognised
            /// call/arg used by the pack. <see cref="ApiVersionRegistry.Baseline"/>
            /// when nothing remarkable is used (i.e. the pack works on
            /// any framework version still supporting baseline).</summary>
            public VersionSlim RequiredVersion;
            /// <summary>All call/arg sites that contribute a non-baseline
            /// <see cref="UsageNote.Required"/>. May be empty when the
            /// pack uses only baseline calls.</summary>
            public List<UsageNote> Notes = new List<UsageNote>();
        }

        public static Report Analyze(LoadedPack pack) {
            Report report = new Report { RequiredVersion = ApiVersionRegistry.Baseline };
            if (pack == null) return report;
            foreach (LoadedFile file in pack.Files) {
                if (file?.Ast == null) continue;
                string fileName = file.AbsolutePath != null
                    ? Path.GetFileName(file.AbsolutePath)
                    : "(unknown)";
                walkBlock(file.Ast, fileName, report);
            }
            return report;
        }

        // ---- AST traversal --------------------------------------------------

        private static void walkBlock(Block block, string fileName, Report report) {
            if (block == null) return;
            foreach (IStatement stmt in block.statements) {
                walkStatement(stmt, fileName, report);
            }
        }

        // Most pack files only have top-level expressions calling
        // build_*, but `if product_exist(...): build_recipe(...)` is a
        // legitimate pattern (guards), and `name = build_*` assignment
        // captures the proto for re-use. We support both so guarded
        // late-version calls still show up in the report.
        private static void walkStatement(IStatement stmt, string fileName, Report report) {
            switch (stmt) {
                case EvaluateStatement ev:
                    walkExpression(ev.Expression, fileName, statementLine(stmt), report);
                    break;
                case AssignmentStatement asn:
                    // Assignment's RHS is what we care about — left side
                    // is just the binding name.
                    walkExpression(asn.Value, fileName, statementLine(stmt), report);
                    break;
                case IfStatement ifs:
                    // Each `elif` / `else` clause is its own IfStatement
                    // queued in the same outer block, so this single recurse
                    // visits the leading clause's body; the sibling clauses
                    // get visited by the outer walkBlock loop on their own.
                    walkBlock(ifs.Block, fileName, report);
                    break;
            }
        }

        private static int statementLine(IStatement stmt) {
            switch (stmt) {
                case AssignmentStatement asn: return asn.StartLine;
                case EvaluateStatement ev:    return ev.StartLine;
                case IfStatement ifs:         return ifs.StartLine;
                default: return 0;
            }
        }

        private static void walkExpression(IExpression expr, string fileName, int line, Report report) {
            if (expr is CallExpression call) {
                visitCall(call, fileName, line, report);
                // Walk into args so a Port(...) nested inside a
                // build_machine(add_ports=[Port(...)]) still gets visited.
                if (call.Arguments != null) {
                    foreach (IArgument arg in call.Arguments) {
                        if (arg?.Expression != null) {
                            walkExpression(arg.Expression, fileName, line, report);
                        }
                    }
                }
            }
            // Other expression kinds (constants, binary ops, list
            // literals, etc.) carry no callable names we'd recognise —
            // skip without recursing further. List-literal-of-Port-calls
            // is handled because the list contains CallExpressions that
            // ultimately call into the args loop via __call__ at runtime,
            // and the AST shape there is also a CallExpression on the
            // list literal's elements that the outer loop has already
            // visited.
        }

        private static void visitCall(CallExpression call, string fileName, int line, Report report) {
            string callName = nameOf(call);
            if (callName == null) return;
            if (!ApiVersionRegistry.Calls.TryGetValue(callName, out ApiCallSpec spec)) return;

            // Call itself.
            bump(report, spec.Since, fileName, line, callName, "");

            // Named args. Positional args don't carry a name in the AST,
            // so we can't map them to registry entries. That's fine —
            // late-added args are almost always named (Python optional
            // kwargs).
            if (call.Arguments == null) return;
            foreach (IArgument arg in call.Arguments) {
                if (arg is NamedArgument named && !string.IsNullOrEmpty(named.Name)) {
                    VersionSlim argSince = spec.SinceForArg(named.Name);
                    if (argSince > spec.Since) {
                        bump(report, argSince, fileName, line, callName, named.Name);
                    }
                }
            }
        }

        private static string nameOf(CallExpression call) {
            // The vast majority of API entry points are simple bare
            // names: build_recipe(...), add_unit_prefab(...), Port(...).
            // Compound paths (a.b.c()) don't match any registry entry
            // so we don't bother trying to resolve them.
            if (call.Calle is VariableExpression ve) return ve.Path;
            return null;
        }

        private static void bump(Report report, VersionSlim required,
                string fileName, int line, string call, string argName) {
            if (required > report.RequiredVersion) {
                report.RequiredVersion = required;
            }
            if (required > ApiVersionRegistry.Baseline) {
                report.Notes.Add(new UsageNote {
                    FileName = fileName,
                    Line     = line,
                    Call     = call,
                    ArgName  = argName,
                    Required = required,
                });
            }
        }
    }
}
