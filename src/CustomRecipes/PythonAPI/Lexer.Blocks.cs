using PythonAPI;
using PythonAPI.Expressions;
using PythonAPI.Statements;
using System.Collections.Generic;

namespace CustomAssets.Python
{
    public partial class Lexer
    {
        private void ParseClass(Block tree)
        {
            Token className = RequireNext(PythonTokens.name);
            List<IExpression> baseClasses = new List<IExpression>();
            if (IsNext(PythonTokens.lparen, out Token _))
            {
                baseClasses.Add(ParseQualifiedName());
                while (IsNext(PythonTokens.next, out Token _))
                {
                    baseClasses.Add(ParseQualifiedName());
                }
                RequireNext(PythonTokens.rparen);
            }
            RequireNext(PythonTokens.block);
            RequireNext(PythonTokens.newline);
            RequireNext(PythonTokens.indent);

            Block block = ParseBlock(tree, PythonTokens.dedent);
            ClasssStatement @class = new ClasssStatement(className, baseClasses, block);
            tree.Add(@class);

            RequireNext(PythonTokens.dedent);
        }

        private void ParseFunction(Block tree)
        {
            Token className = RequireNext(PythonTokens.name);
            RequireNext(PythonTokens.lparen);
            List<Token> arguments = NextList0(PythonTokens.name, PythonTokens.next);
            RequireNext(PythonTokens.rparen);
            RequireNext(PythonTokens.block);

            // is function definable as single statement?
            RequireNext(PythonTokens.newline);
            RequireNext(PythonTokens.indent);

            Block block = ParseBlock(tree, PythonTokens.dedent);
            FunctionStatement @class = new FunctionStatement(className, arguments, block);
            tree.Add(@class);

            RequireNext(PythonTokens.dedent);
        }

        private void ParseIf(Block tree, int headerLine)
        {
            IExpression condition = ParseExpression();
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                IfStatement @class = new IfStatement(condition, block) {
                    StartLine = headerLine,
                    EndLine = lastBodyLine(headerLine)
                };
                tree.Add(@class);

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                // Single-line `if cond: stmt` — header and body share a line.
                Block block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new IfStatement(condition, block) {
                    StartLine = headerLine,
                    EndLine = lastBodyLine(headerLine)
                });
            }
        }

        private void ParseElIf(IfStatement ifs, Block tree, int headerLine)
        {
            IExpression condition = ParseExpression();
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                IfStatement @class = new IfStatement(ifs, condition, block) {
                    StartLine = headerLine,
                    EndLine = lastBodyLine(headerLine)
                };
                tree.Add(@class);

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                var block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new IfStatement(ifs, condition, block) {
                    StartLine = headerLine,
                    EndLine = lastBodyLine(headerLine)
                });
            }
        }

        private void ParseElse(IfStatement ifs, Block tree, int headerLine)
        {
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                IfStatement @class = new IfStatement(ifs, block) {
                    StartLine = headerLine,
                    EndLine = lastBodyLine(headerLine)
                };
                tree.Add(@class);

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                Block block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new IfStatement(ifs, block) {
                    StartLine = headerLine,
                    EndLine = lastBodyLine(headerLine)
                });
            }
        }

        // Pick the line of the body's last non-trivial token after ParseBlock
        // returned. m_lastNonTrivialToken tracks all dequeues except whitespace
        // (see Dequeue() in Lexer.cs); reading it AFTER the body parse means we
        // see the last actual token of the conditional's body. Falls back to
        // the header line for an empty body — defensive only; the grammar
        // wouldn't normally allow that.
        private int lastBodyLine(int headerLine) {
            return m_lastNonTrivialToken != null ? m_lastNonTrivialToken.line : headerLine;
        }
    }
}
