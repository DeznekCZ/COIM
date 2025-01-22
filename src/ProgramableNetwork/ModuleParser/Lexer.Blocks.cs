using Mafi.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Python
{
    public partial class Lexer
    {
        private void ParseClass(Block tree)
        {
            Token className = RequireNext(PythonTokens.name, PythonTokens.space);
            List<QualifiedName> baseClasses = new List<QualifiedName>();
            if (IsNext(PythonTokens.lparen, out Token _, PythonTokens.space))
            {
                baseClasses.Add(ParseQualifiedName(PythonTokens.space));
                while (IsNext(PythonTokens.next, out Token _, PythonTokens.space))
                {
                    baseClasses.Add(ParseQualifiedName(PythonTokens.space));
                }
                RequireNext(PythonTokens.rparen, PythonTokens.space);
            }
            RequireNext(PythonTokens.block, PythonTokens.space);
            RequireNext(PythonTokens.newline, PythonTokens.space);

            while (IsNext(PythonTokens.newline, out Token _, PythonTokens.space))
                continue;// read non empty lines

            var indentation = RequireNext(PythonTokens.space).value;
            while (IsNext(PythonTokens.space, out Token space))
                indentation += space.value;

            Block block = ParseBlock(tree, indentation);
            ClasssStatement @class = new ClassDefinition(className, baseClasses, block);
            tree.Add(@class);
        }

        private void ParseFunction(Block tree)
        {
            Token className = RequireNext(PythonTokens.name, PythonTokens.space);
            RequireNext(PythonTokens.lparen, PythonTokens.space);
            List<Token> arguments = NextList0(PythonTokens.name, PythonTokens.next, PythonTokens.space);
            RequireNext(PythonTokens.rparen, PythonTokens.space);
            RequireNext(PythonTokens.block, PythonTokens.space);
            RequireNext(PythonTokens.newline, PythonTokens.space);

            while (IsNext(PythonTokens.newline, out Token _, PythonTokens.space))
                continue;// read non empty lines

            var indentation = RequireNext(PythonTokens.space).value;
            while (IsNext(PythonTokens.space, out Token space))
                indentation += space.value;

            Block block = ParseBlock(tree, indentation);
            Function @class = new Function(className, arguments, block);
            tree.Add(@class);
        }

        private void ParseIf(Block tree)
        {
            IExpression condition = ParseExpression(PythonTokens.space);
            RequireNext(PythonTokens.block, PythonTokens.space);
            RequireNext(PythonTokens.newline, PythonTokens.space);

            while (IsNext(PythonTokens.newline, out Token _, PythonTokens.space))
                continue;// read non empty lines

            var indentation = RequireNext(PythonTokens.space).value;
            while (IsNext(PythonTokens.space, out Token space))
                indentation += space.value;

            Block block = ParseBlock(tree, indentation);
            IfStatement @class = new IfStatement(condition, block);
            tree.Add(@class);
        }

        private void ParseElIf(IfStatement ifs, Block tree)
        {
            IExpression condition = ParseExpression(PythonTokens.space);
            RequireNext(PythonTokens.block, PythonTokens.space);
            RequireNext(PythonTokens.newline, PythonTokens.space);

            while (IsNext(PythonTokens.newline, out Token _, PythonTokens.space))
                continue;// read non empty lines

            var indentation = RequireNext(PythonTokens.space).value;
            while (IsNext(PythonTokens.space, out Token space))
                indentation += space.value;

            Block block = ParseBlock(tree, indentation);
            IfStatement @class = new IfStatement(ifs, condition, block);
            tree.Add(@class);
        }

        private void ParseElse(IfStatement ifs, Block tree)
        {
            RequireNext(PythonTokens.block, PythonTokens.space);
            RequireNext(PythonTokens.newline, PythonTokens.space);

            while (IsNext(PythonTokens.newline, out Token _, PythonTokens.space))
                continue;// read non empty lines

            var indentation = RequireNext(PythonTokens.space).value;
            while (IsNext(PythonTokens.space, out Token space))
                indentation += space.value;

            Block block = ParseBlock(tree, indentation);
            IfStatement @class = new IfStatement(ifs, block);
            tree.Add(@class);
        }
    }
}
