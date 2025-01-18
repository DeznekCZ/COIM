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
        private LinkedList<Token> enumerator;

        public static Block Parse(Token[] tokens)
        {
            if (tokens == null || tokens.Length == 0)
                return new Block();

            Lexer lexer = new Lexer();
            lexer.enumerator = new LinkedList<Token>(tokens);
            return lexer.ParseTree();
        }

        private Block ParseTree()
        {
            return ParseBlock(null, "");
        }

        private Block ParseBlock(Block parentTree, string indentation)
        {
            Block tree = new Block(parentTree);
            while (enumerator.Count > 0)
            {
                Token token = Dequeue();
                switch (token.type)
                {
                    case PythonTokens.from:
                        QualifiedName p = ParseQualifiedName(PythonTokens.space);
                        RequireNext(PythonTokens.import, ignore: PythonTokens.space);
                        Import import = new Import(p, NextList(PythonTokens.name, next: PythonTokens.next, ignore: PythonTokens.space));
                        tree.Add(import);
                        RequireNext(PythonTokens.newline, PythonTokens.space);
                        break;

                    case PythonTokens.classp:
                        ParseClass(tree);
                        break;

                    case PythonTokens.def:
                        ParseFunction(tree);
                        break;

                    case PythonTokens.ifp:
                        ParseIf(tree);
                        break;

                    case PythonTokens.elif:
                        if (!(tree.statements.Last() is IfStatement ifs1))
                            throw new InvalidOperationException($"elif is allowed only after if or elif statement: {token}");

                        ParseElIf(ifs1, tree);
                        break;

                    case PythonTokens.elsep:
                        if (!(tree.statements.Last() is IfStatement ifs2))
                            throw new InvalidOperationException($"else is allowed only after if or elif statement: {token}");

                        ParseElse(ifs2, tree);
                        break;

                    case PythonTokens.name:
                        // Assignment
                        Revert(token);
                        IExpression leftExpression = ParseExpression(PythonTokens.space);
                        while (IsNext(PythonTokens.set, out Token _, PythonTokens.space))
                        {
                            tree.Add(ParseAssignment(leftExpression));
                            break;
                        }
                        IsNext(PythonTokens.newline, out Token _, PythonTokens.space);
                        break;

                    case PythonTokens.newline:
                        break;

                    case PythonTokens.space:
                        Revert(token);

                        var nextIndentation = RequireNext(PythonTokens.space).value;
                        while (IsNext(PythonTokens.space, out Token space))
                            nextIndentation += space.value;

                        if (nextIndentation != indentation)
                        {
                            return tree;
                        }
                        break;

                    case PythonTokens.undefined:
                    default:
                        throw new NotImplementedException($"undefined: {token}");
                }
            }
            return tree;
        }

        private void Revert(Token token)
        {
            enumerator.AddFirst(token);
        }

        private Assignment ParseAssignment(IExpression qualifiedName)
        {
            return new Assignment(qualifiedName, ParseExpression(PythonTokens.space));
        }

        private IExpression ParseExpression(params PythonTokens[] ignore)
        {
            if (IsNext(PythonTokens.lparen, out Token _, ignore))
            {
                IExpression expression = ParseExpression(PythonTokens.space, PythonTokens.newline);
                while (IsNext(PythonTokens.next, out Token _, ignore))
                { // tuple
                    throw new NotImplementedException("tuples are not implemented");
                }
                RequireNext(PythonTokens.rparen, PythonTokens.space, PythonTokens.newline);
                return expression;
            }

            if (IsNext(PythonTokens.str, out Token str, ignore))
            {
                return new StringConstant(str);
            }

            if (IsNext(PythonTokens.number, out Token num, ignore))
            {
                return new NumberConstant(num);
            }

            if (IsNext(PythonTokens.llist, out Token listStart, ignore))
            {
                IExpression argument = ParseExpression(PythonTokens.space, PythonTokens.newline);
                List<IExpression> listItems = new List<IExpression>() { argument };
                while (IsNext(PythonTokens.next, out Token _, ignore))
                {
                    listItems.Add(ParseExpression(PythonTokens.space, PythonTokens.newline));
                }
                var listEnd = RequireNext(PythonTokens.rlist, PythonTokens.space, PythonTokens.newline);
                return DecorateExpression(new ListValue(listStart, listEnd, listItems), ignore);
            }

            if (IsNext(PythonTokens.ldict, out Token dictStart, ignore))
            {
                throw new NotImplementedException("dictionary parsiong is not implemented");

                IExpression argument = ParseExpression(PythonTokens.space, PythonTokens.newline);
                List<IExpression> listItems = new List<IExpression>() { argument };
                while (IsNext(PythonTokens.next, out Token _, ignore))
                {
                    listItems.Add(ParseExpression(PythonTokens.space, PythonTokens.newline));
                }
                var dictEnd = RequireNext(PythonTokens.rdict, PythonTokens.space, PythonTokens.newline);
                return DecorateExpression(new ListValue(dictStart, dictEnd, listItems), ignore);
            }

            if (IsNext(PythonTokens.none, out Token none, ignore))
            {
                return DecorateExpression(new NoneConst(none), ignore);
            }

            if (IsNextOf(new PythonTokens[]
            {
                PythonTokens.btrue,
                PythonTokens.bfalse
            }, out Token boolToken, ignore))
            {
                return DecorateExpression(new BooleanConst(boolToken), ignore);
            }

            return DecorateExpression(ParseQualifiedName(ignore), ignore);
        }

        private IExpression DecorateExpression(IExpression finalExpression, PythonTokens[] ignore)
        {
            if (IsNext(PythonTokens.lparen, out Token _, PythonTokens.space))
            {
                if (IsNext(PythonTokens.rparen, out Token _, PythonTokens.space))
                    return new Call(finalExpression, new List<IExpression>());

                IExpression argument = ParseExpression(PythonTokens.space, PythonTokens.newline);
                List<IExpression> arguments = new List<IExpression>() { argument };
                while (IsNext(PythonTokens.next, out Token _, ignore))
                {
                    arguments.Add(ParseExpression(PythonTokens.space, PythonTokens.newline));
                }
                RequireNext(PythonTokens.rparen, PythonTokens.space, PythonTokens.newline);
                return DecorateExpression(new Call(finalExpression, arguments), ignore);
            }

            if (IsNext(PythonTokens.isp, out Token _, ignore))
            {
                bool not = IsNext(PythonTokens.not, out Token _, PythonTokens.space);
                return DecorateExpression(new IsExpression(finalExpression, not, ParseExpression(PythonTokens.space)), ignore);
            }

            if (IsNextOf(new PythonTokens[] {
                PythonTokens.plus,
                PythonTokens.minus,
                PythonTokens.mul,
                PythonTokens.div,
                PythonTokens.divint
            }, out Token arith, ignore))
            {
                return DecorateExpression(new ArithmeticExpression(finalExpression, arith), ignore);
            }

            if (IsNextOf(new PythonTokens[] {
                PythonTokens.eq,
                PythonTokens.neq,
                PythonTokens.gr,
                PythonTokens.gre,
                PythonTokens.lr,
                PythonTokens.lre
            }, out Token comp, ignore))
            {
                return DecorateExpression(new CompareExpression(finalExpression, comp), ignore);
            }

            if (IsNextOf(new PythonTokens[] {
                PythonTokens.and,
                PythonTokens.or,
            }, out Token boolean, ignore))
            {
                return DecorateExpression(new BooleanExpression(finalExpression, boolean), ignore);
            }

            return finalExpression;
        }

        private List<Token> NextList(PythonTokens type, PythonTokens next, params PythonTokens[] ignore)
        {
            List<Token> list = new List<Token>();

            Token first = RequireNext(type, ignore);
            list.Add(first);

            while(IsNext(next, out Token _, ignore))
                list.Add(RequireNext(type, ignore));

            return list;
        }

        private List<Token> NextList0(PythonTokens type, PythonTokens next, params PythonTokens[] ignore)
        {
            List<Token> list = new List<Token>();

            if (!IsNext(type, out Token first, ignore))
                return list;

            list.Add(first);

            while(IsNext(next, out Token _, ignore))
                list.Add(RequireNext(type, ignore));

            return list;
        }

        private Token RequireNext(PythonTokens type, params PythonTokens[] ignore)
        {
            var token = Dequeue();
            while (ignore.Contains(token.type))
                token = Dequeue();
            if (token.type != type)
                throw new InvalidOperationException($"Expected token '{type}', got: {token}");
            return token;
        }

        private bool IsNext(PythonTokens type, out Token token, params PythonTokens[] ignore)
        {
            if (enumerator.Count == 0)
            {
                token = Token.EOF;
                return false;
            }

            Stack<Token> stack = new Stack<Token>();
            token = Dequeue();
            stack.Push(token);
            while (ignore.Contains(token.type))
            {
                token = Dequeue();
                stack.Push(token);
            }
            if (token.type != type)
            { // revert

                while (stack.Count > 0)
                    enumerator.AddFirst(stack.Pop());

                token = null;
                return false;
            }
            return true;
        }

        private bool IsNextOf(PythonTokens[] type, out Token token, params PythonTokens[] ignore)
        {
            if (enumerator.Count == 0)
            {
                token = null;
                return false;
            }

            Stack<Token> stack = new Stack<Token>();
            token = Dequeue();
            stack.Push(token);
            while (ignore.Contains(token.type))
            {
                token = Dequeue();
                stack.Push(token);
            }
            if (!type.Contains(token.type))
            { // revert

                while (stack.Count > 0)
                    enumerator.AddFirst(stack.Pop());

                token = null;
                return false;
            }
            return true;
        }

        private QualifiedName ParseQualifiedName(params PythonTokens[] ignore)
        {
            QualifiedName path = new QualifiedName();
            Token name = RequireNext(PythonTokens.name, ignore);
            path.names.Add(name);

            while (IsNext(PythonTokens.dot, out Token _, ignore))
            {
                Token nextname = RequireNext(PythonTokens.name, ignore);
                path.names.Add(nextname);
            }
            return path;
        }

        private Token Dequeue()
        {
            Token token = enumerator.First();
            enumerator.RemoveFirst();
            return token;
        }
    }
}
