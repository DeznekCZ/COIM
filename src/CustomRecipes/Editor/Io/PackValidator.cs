using System.Collections.Generic;
using System.IO;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using PythonAPI;
using PythonAPI.Arguments;
using PythonAPI.Expressions;
using PythonAPI.Statements;

namespace CustomAssets.Editor.Io {

    /// <summary>
    /// Lightweight post-load AST checks. The first (and currently only)
    /// check catches use-before-definition of a Python variable in the
    /// same source file: <c>material = filter_media_mat</c> on line 12
    /// is invalid when <c>filter_media_mat = add_loose_product_material(...)</c>
    /// sits on line 30, since the COI Python runtime walks the file
    /// top-to-bottom and would NameError on line 12 before ever reaching
    /// the assignment.
    ///
    /// The check runs per file (variables are file-scoped in the
    /// CustomAssets pack model — there's no cross-file binding) and
    /// considers SIMPLE identifiers only (a bare name like
    /// <c>filter_media_mat</c>; dotted paths such as <c>Ids.Products.X</c>
    /// are typed references, not local variables, and are skipped).
    ///
    /// Conservative on if/elif/else scoping: any assignment anywhere in
    /// the file counts as a definition at its line. Python doesn't have
    /// block scope, so a variable assigned inside an if-clause is
    /// visible after the if at runtime — and reordering would only ever
    /// produce false negatives, never false positives.
    /// </summary>
    public static class PackValidator {

        /// Run every check, return the combined issue list. Caller is
        /// expected to attach the result to <see cref="PackModel.Issues"/>.
        public static List<PackIssue> Validate(LoadedPack pack) {
            List<PackIssue> issues = new List<PackIssue>();
            if (pack == null) return issues;
            foreach (LoadedFile file in pack.Files) {
                checkUseBeforeDefinition(file, issues);
            }
            return issues;
        }

        private static void checkUseBeforeDefinition(LoadedFile file, List<PackIssue> issues) {
            if (file?.Ast == null) return;

            // Pass 1: gather every assignment's name + earliest defining
            // line across the whole file (including nested if/elif/else
            // bodies — see class doc for the scoping rationale).
            Dictionary<string, int> defLine = new Dictionary<string, int>(System.StringComparer.Ordinal);
            collectAssignments(file.Ast.statements, defLine);

            // Pass 2: walk every statement; for each VariableExpression
            // whose name is in defLine but whose containing-statement line
            // is BEFORE the defining line, record an issue. The statement
            // line is the natural "where the reference is read" — the
            // expression itself doesn't carry its own line, so we
            // attribute to the statement that holds it.
            walkForReferences(file.Ast.statements, defLine, file.AbsolutePath, issues);
        }

        private static void collectAssignments(
                List<IStatement> stmts, Dictionary<string, int> defLine) {
            foreach (IStatement s in stmts) {
                if (s is AssignmentStatement asn && !string.IsNullOrEmpty(asn.Name)
                        && !asn.Name.Contains(".")) {
                    if (!defLine.TryGetValue(asn.Name, out int existing) || asn.StartLine < existing) {
                        defLine[asn.Name] = asn.StartLine;
                    }
                }
                if (s is IfStatement ifStmt && ifStmt.Block?.statements != null) {
                    collectAssignments(ifStmt.Block.statements, defLine);
                }
            }
        }

        private static void walkForReferences(
                List<IStatement> stmts, Dictionary<string, int> defLine,
                string sourceFile, List<PackIssue> issues) {
            foreach (IStatement s in stmts) {
                if (s is AssignmentStatement asn) {
                    // selfName guards `filter_media_mat = some_call(filter_media_mat)`
                    // — the RHS reference shouldn't be flagged against the LHS
                    // it's about to define (Python rebind semantics would let
                    // this work if the prior binding existed; if not, the user
                    // sees a different runtime error, not what we'd flag).
                    walkExpr(asn.Value, asn.StartLine, defLine, sourceFile, asn.Name, issues);
                    continue;
                }
                if (s is EvaluateStatement ev) {
                    walkExpr(ev.Expression, ev.StartLine, defLine, sourceFile, null, issues);
                    continue;
                }
                if (s is IfStatement ifStmt) {
                    if (ifStmt.Condition != null) {
                        walkExpr(ifStmt.Condition, ifStmt.StartLine, defLine, sourceFile, null, issues);
                    }
                    if (ifStmt.Block?.statements != null) {
                        walkForReferences(ifStmt.Block.statements, defLine, sourceFile, issues);
                    }
                }
            }
        }

        // Recursively walk an expression looking for VariableExpression
        // references that resolve against defLine but appear before
        // their defining line.
        private static void walkExpr(
                IExpression expr, int refLine, Dictionary<string, int> defLine,
                string sourceFile, string selfName, List<PackIssue> issues) {
            if (expr == null) return;
            if (expr is VariableExpression v) {
                string p = v.Path;
                if (string.IsNullOrEmpty(p) || p.Contains(".")) return;
                if (selfName != null && p == selfName) return;
                if (defLine.TryGetValue(p, out int defL) && refLine < defL) {
                    issues.Add(new PackIssue {
                        SourceFile = sourceFile,
                        Line = refLine,
                        Message = "Variable '" + p + "' is used at line " + refLine
                                  + " but defined later at line " + defL
                                  + " in " + Path.GetFileName(sourceFile)
                                  + ". The COI runtime walks files top-to-bottom"
                                  + " - move the assignment above the reference"
                                  + " (or rename the variable) before saving."
                    });
                }
                return;
            }
            if (expr is CallExpression c) {
                walkExpr(c.Calle, refLine, defLine, sourceFile, selfName, issues);
                if (c.Arguments != null) {
                    foreach (IArgument arg in c.Arguments) {
                        walkExpr(arg.Expression, refLine, defLine, sourceFile, selfName, issues);
                    }
                }
                return;
            }
            if (expr is PyTuple t) {
                if (t.ExpressionLists != null) {
                    foreach (IExpression item in t.ExpressionLists) {
                        walkExpr(item, refLine, defLine, sourceFile, selfName, issues);
                    }
                }
                return;
            }
            if (expr is ListExpression le) {
                if (le.Items != null) {
                    foreach (IExpression item in le.Items) {
                        walkExpr(item, refLine, defLine, sourceFile, selfName, issues);
                    }
                }
                return;
            }
            // Other expression kinds (NumberConstant, StringConstant,
            // BooleanConst, NoneConst, NegativeExpression, binary ops, etc.)
            // either don't contain variable references at all, or expose
            // their children only through non-public fields. The cases
            // above cover the shapes that hold user-facing variable refs
            // in the build_*/add_* call surface (call arguments, tuple
            // arguments like color=(R,G,B), and list arguments like
            // entities=[id1, id2]).
        }
    }
}
