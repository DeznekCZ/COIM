using System.Collections.Generic;

namespace ProgramableNetwork.Python
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

        private void ParseIf(Block tree)
        {
            IExpression condition = ParseExpression();
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                IfStatement @class = new IfStatement(condition, block);
                tree.Add(@class);

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                Block block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new IfStatement(condition, block));
            }
        }

        private void ParseElIf(IfStatement ifs, Block tree)
        {
            IExpression condition = ParseExpression();
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                IfStatement @class = new IfStatement(ifs, condition, block);
                tree.Add(@class);

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                var block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new IfStatement(ifs, condition, block));
            }
        }

        // `for VAR in EXPR:` followed by an indented block (or single inline
        // statement after the colon, mirroring the same dual form ParseIf
        // accepts).  VAR is bound to a fresh identifier name; the actual
        // assignment happens at runtime inside ForStatement.Execute.
        private void ParseFor(Block tree)
        {
            Token variable = RequireNext(PythonTokens.name);
            RequireNext(PythonTokens.ink);
            IExpression iterable = ParseExpression();
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                tree.Add(new ForStatement(variable.value, iterable, block));

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                Block block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new ForStatement(variable.value, iterable, block));
            }
        }

        // `while EXPR:` block; same dual form as ParseFor / ParseIf.
        private void ParseWhile(Block tree)
        {
            IExpression condition = ParseExpression();
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                tree.Add(new WhileStatement(condition, block));

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                Block block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new WhileStatement(condition, block));
            }
        }

        private void ParseElse(IfStatement ifs, Block tree)
        {
            RequireNext(PythonTokens.block);

            if (IsNext(PythonTokens.newline, out Token _))
            {
                RequireNext(PythonTokens.indent);

                Block block = ParseBlock(tree, PythonTokens.dedent);
                IfStatement @class = new IfStatement(ifs, block);
                tree.Add(@class);

                RequireNext(PythonTokens.dedent);
            }
            else
            {
                Block block = ParseBlock(tree, PythonTokens.newline);
                tree.Add(new IfStatement(ifs, block));
            }
        }
    }
}
