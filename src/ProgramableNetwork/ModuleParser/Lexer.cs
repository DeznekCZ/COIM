using Mafi.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Python
{
    /// <summary>
    /// Following grammar, but cutted:
    /// <br/>https://docs.python.org/3/reference/grammar.html
    /// </summary>
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
                            leftExpression = ParseAssignment(leftExpression);
                            break;
                        }
                        IsNext(PythonTokens.newline, out Token _, PythonTokens.space);
                        tree.Add(leftExpression);
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
            return disjunct();
        }

        private Func<IExpression> And(params Func<IExpression>[] exp)
        {
            return ExpressionGraph.And(exp);
        }

        private ExpressionGraph.AndVisitor And(params PythonTokens[] exp)
        {
            return ExpressionGraph.And(exp.Select(Token).ToArray());
        }

        private ExpressionGraph.OrVisitor Or(params Func<IExpression>[] exp)
        {
            return ExpressionGraph.Or(exp);
        }

        private ExpressionGraph.OrVisitor Or(Func<ExpressionGraph, IExpression> visitor, params Func<ExpressionGraph>[] exp)
        {
            return ExpressionGraph.Or(exp);
        }

        private ExpressionGraph Token(params PythonTokens[] exp)
        {
            return ExpressionGraph.Or(exp.Select(Token).ToArray());
        }

        private ExpressionGraph Multiple(ExpressionGraph exp)
        {
            return ExpressionGraph.Multiple(exp);
        }

        private ExpressionGraph Maybe(ExpressionGraph exp)
        {
            return ExpressionGraph.Maybe(exp);
        }

        private ExpressionGraph.TokenGraph Token(PythonTokens token)
        {
            return ExpressionGraph.Token(token);
        }

        private IExpression disjunct()
        {
            IExpression con = conjunct();
            while (IsNext(PythonTokens.or, out Token _, PythonTokens.space))
            {
                con = new OrExpression(con, conjunct());
            }
            return con;
        }

        private IExpression conjunct()
        {
            IExpression inv = invert();
            while (IsNext(PythonTokens.or, out Token _, PythonTokens.space))
            {
                inv = new AndExpression(inv, invert());
            }
            return inv;
        }

        private IExpression invert()
        {
            bool direction = false;
            while (IsNext(PythonTokens.not, out Token _, PythonTokens.space))
            {
                direction = !direction;
            }
            if (direction)
                return new NotExpression(compare());
            else
                return compare();
        }

        private IExpression compare()
        {
            IExpression bitvise = bitviseor();
            while (IsNextOf(new PythonTokens[]
            {
                PythonTokens.eq,
                PythonTokens.neq,
                PythonTokens.lre,
                PythonTokens.lr,
                PythonTokens.gre,
                PythonTokens.gr,
                PythonTokens.not,
                PythonTokens.ink,
                PythonTokens.isp
            }, out Token operat, PythonTokens.space))
            {
                if (operat.type == PythonTokens.not)
                {
                    RequireNext(PythonTokens.ink, PythonTokens.space);
                    bitvise = new NotExpression(new InExpression(bitvise, bitviseor()));
                }
                else if (operat.type == PythonTokens.isp)
                {
                    if (IsNext(PythonTokens.not, out Token _, PythonTokens.space))
                    {
                        bitvise = new NotExpression(new IsExpression(bitvise, bitviseor()));
                    }
                }
                else
                {
                    switch (operat.type)
                    {
                        case PythonTokens.eq:
                            bitvise = new EqualExpression(bitvise, bitvisexor());
                            break;
                        case PythonTokens.neq:
                            bitvise = new NotExpression(new EqualExpression(bitvise, bitvisexor()));
                            break;
                        case PythonTokens.lre:
                            bitvise = new LowerEqualExpression(bitvise, bitvisexor());
                            break;
                        case PythonTokens.gre:
                            bitvise = new GreaterEqualExpression(bitvise, bitvisexor());
                            break;
                        case PythonTokens.lr:
                            bitvise = new LowerExpression(bitvise, bitvisexor());
                            break;
                        case PythonTokens.gr:
                            bitvise = new GreaterExpression(bitvise, bitvisexor());
                            break;
                        default:
                            break;
                    }
                }
            }
            return bitvise;
        }

        private IExpression bitviseor()
        {
            List<IExpression> ors = new List<IExpression>
            {
                bitvisexor()
            };
            while (IsNext(PythonTokens.bitor, out Token _, PythonTokens.space))
            {
                ors.Add(bitviseor());
            }
            if (ors.Count == 1) return ors[0];

            IExpression f = ors.Last();
            for (int i = ors.Count - 2; i >= 0; i--)
            {
                f = new BitOrExpression(ors[i], f);
            }
            return f;
        }

        private IExpression bitvisexor()
        {
            List<IExpression> ands = new List<IExpression>
            {
                bitviseand()
            };
            while (IsNext(PythonTokens.bitxor, out Token _, PythonTokens.space))
            {
                ands.Add(bitviseand());
            }
            if (ands.Count == 1) return ands[0];

            IExpression f = ands.Last();
            for (int i = ands.Count - 2; i >= 0; i--)
            {
                f = new BitXorExpression(ands[i], f);
            }
            return f;
        }

        private IExpression bitviseand()
        {
            List<IExpression> shifts = new List<IExpression>
            {
                bitviseshift()
            };
            while (IsNext(PythonTokens.bitand, out Token _, PythonTokens.space))
            {
                shifts.Add(bitviseshift());
            }
            if (shifts.Count == 1) return shifts[0];

            IExpression f = shifts.Last();
            for (int i = shifts.Count - 2; i >= 0; i--)
            {
                f = new BitXorExpression(shifts[i], f);
            }
            return f;
            return Or(
                    And(bitviseor(), Token(PythonTokens.bitand), bitviseshift()),
                    bitviseshift()
                );
        }

        private IExpression bitviseshift()
        {
            return Or(
                    And(bitviseshift(), Token(PythonTokens.shiftl, PythonTokens.shiftr), sum()),
                    sum()
                );
        }

        private IExpression sum()
        {
            return Or(
                    And(sum(), Token(PythonTokens.plus, PythonTokens.minus), term()),
                    term()
                );
        }

        private IExpression term()
        {
            return Or(
                    And(term(), Token(
                        PythonTokens.mul,
                        PythonTokens.div,
                        PythonTokens.divint,
                        PythonTokens.mod
                        // what is @
                    ), factor()),
                    factor()
                );
        }

        private IExpression factor()
        {
            return Or(
                And(
                    Token(
                        PythonTokens.plus,
                        PythonTokens.minus,
                        PythonTokens.invert
                    ), factor()
                ),
                power()
            );
        }

        private IExpression power()
        {
            return Or(
                    And(primary(), Token(PythonTokens.power), factor()),
                    primary()
                );
        }

        private LexerResult primary()
        {
            return Or(
                    And(primary(), Token(PythonTokens.dot), Token(PythonTokens.name)),
                    And(primary(), Token(PythonTokens.lparen), Token(PythonTokens.name), Token(PythonTokens.rparen)),
                    And(primary(), Token(PythonTokens.llist), Token(PythonTokens.name), Token(PythonTokens.rlist)),
                    atom()
                );
        }

        private LexerResult atom()
        {
            return (new LexerState(this) /
                PythonTokens.name /
                PythonTokens.btrue /
                PythonTokens.bfalse /
                PythonTokens.none /
                PythonTokens.str /
                fstring /
                (new LexerState(this) + primary + PythonTokens.lparen - arguments + PythonTokens.rparen) /
                (new LexerState(this) + primary + PythonTokens.llist + PythonTokens.name + PythonTokens.rlist)
            ).visit();
        }

        private IExpression arguments()
        {
            return And(disjunct(), Maybe(And(Token(PythonTokens.next), disjunct())));
        }

        private IExpression fstring()
        {
            return Token(PythonTokens.str); //TODO
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
                token = Python.Token.EOF;
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
