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
                        IExpression p = primary();
                        RequireNext(PythonTokens.import, ignore: PythonTokens.space);
                        ImportStatement import = new ImportStatement(p, NextList(PythonTokens.name, next: PythonTokens.next, ignore: PythonTokens.space));
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
            int direction = 0;
            while (IsNext(PythonTokens.not, out Token _, PythonTokens.space))
            {
                direction++;
            }
            IExpression expression = compare();
            for (int i = 0; i < direction; i++)
            {
                expression = new NotExpression(expression);
            }
            return expression;
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
        }

        private IExpression bitviseshift()
        {
            List<(PythonTokens, IExpression) > shifts = new List<(PythonTokens, IExpression)>
            {
                (0, sum())
            };
            while (IsNextOf(new PythonTokens[]{ PythonTokens.shiftl, PythonTokens.shiftr }, out Token shift, PythonTokens.space))
            {
                shifts.Add((shift.type, sum()));
            }
            if (shifts.Count == 1) return shifts[0].Item2;

            IExpression f = shifts.Last().Item2;
            for (int i = shifts.Count - 2; i >= 0; i--)
            {
                var shiftDrirection = shifts[i+1].Item1;
                if (shiftDrirection == PythonTokens.shiftl)
                {
                    f = new ShiftLeftExpression(shifts[i].Item2, f);
                }
                else
                {
                    f = new ModExpression(shifts[i].Item2, f);
                }
            }
            return f;
        }

        private IExpression sum()
        {
            List<(PythonTokens, IExpression)> sums = new List<(PythonTokens, IExpression)>
            {
                (0, term())
            };
            while (IsNextOf(new PythonTokens[] { PythonTokens.plus, PythonTokens.minus }, out Token shift, PythonTokens.space))
            {
                sums.Add((shift.type, term()));
            }
            if (sums.Count == 1) return sums[0].Item2;

            IExpression f = sums.Last().Item2;
            for (int i = sums.Count - 2; i >= 0; i--)
            {
                var shiftDrirection = sums[i + 1].Item1;
                if (shiftDrirection == PythonTokens.plus)
                {
                    f = new SumExpression(sums[i].Item2, f);
                }
                else
                {
                    f = new ModExpression(sums[i].Item2, f);
                }
            }
            return f;
        }

        private IExpression term()
        {
            List<(PythonTokens, IExpression)> sums = new List<(PythonTokens, IExpression)>
            {
                (0, factor())
            };
            while (IsNextOf(new PythonTokens[] { PythonTokens.mul, PythonTokens.div, PythonTokens.divint , PythonTokens.mod }, out Token operToken, PythonTokens.space))
            {
                sums.Add((operToken.type, factor()));
            }
            if (sums.Count == 1) return sums[0].Item2;

            IExpression f = sums.Last().Item2;
            for (int i = sums.Count - 2; i >= 0; i--)
            {
                var oper = sums[i + 1].Item1;
                if (oper == PythonTokens.mul)
                {
                    f = new MulExpression(sums[i].Item2, f);
                }
                else if (oper == PythonTokens.div)
                {
                    f = new DivExpression(sums[i].Item2, f);
                }
                else if (oper == PythonTokens.divint)
                {
                    f = new DivIntExpression(sums[i].Item2, f);
                }
                else
                {
                    f = new ModExpression(sums[i].Item2, f);
                }
            }
            return f;
        }

        private IExpression factor()
        {
            Stack<PythonTokens> operators = new Stack<PythonTokens>();
            while (IsNextOf(new PythonTokens[] {
                        PythonTokens.plus,
                        PythonTokens.minus,
                        PythonTokens.invert
            }, out Token token, PythonTokens.space)) {
                operators.Push(PythonTokens.plus);
            }
            IExpression expression = power();
            while (operators.Count > 0) {
                switch (operators.Pop())
                {
                    case PythonTokens.plus:
                        expression = new PositiveExpression(expression);
                        break;
                    case PythonTokens.minus:
                        expression = new NegativeExpression(expression);
                        break;
                    case PythonTokens.invert:
                        expression = new InvertExpression(expression);
                        break;
                    default: throw new NotImplementedException();
                };
            }
            return expression;
        }

        private IExpression power()
        {
            IExpression expression = primary();
            while (IsNext(PythonTokens.power, out Token _, PythonTokens.space))
            {
                expression = new PowerExpression(expression, factor());
            }
            return expression;
        }

        private IExpression primary()
        {
            IExpression expression = atom();
            while(IsNextOf(new PythonTokens[]
            {
                PythonTokens.dot,
                PythonTokens.lparen,
                PythonTokens.llist,
            }, out Token member, PythonTokens.space))
            {
                if (member.type == PythonTokens.dot)
                {
                    var nameToken = RequireNext(PythonTokens.name, PythonTokens.space);
                    expression = new PropertyExpression(expression, nameToken.value);
                }
                else if (member.type == PythonTokens.lparen)
                {
                    expression = new CallExpression(expression, arguments());
                }
                else if (member.type == PythonTokens.rparen)
                {
                    expression = new IndexExpression(expression, range());
                }
            }
            return expression;
        }

        private IExpression atom()
        {
            Token decide = AnyNext(PythonTokens.space);
            switch (decide.type)
            {
                case PythonTokens.name:
                    return new VariableExpression(decide.value);
                case PythonTokens.btrue:
                    return new BooleanConst(decide);
                case PythonTokens.bfalse:
                    return new BooleanConst(decide);
                case PythonTokens.none:
                    return new NoneConst(decide);
                case PythonTokens.number:
                    return new NumberConstant(decide);
                case PythonTokens.str:
                case PythonTokens.mstr:
                    return new StringConstant(decide);
                case PythonTokens.fstrbegin:
                    Revert(decide);
                    return fstring();
                case PythonTokens.lparen:
                    // TODO tuple
                    IExpression expression = ParseExpression(PythonTokens.space);
                    RequireNext(PythonTokens.rparen, PythonTokens.space);
                    return expression;
                case PythonTokens.llist:
                    // TODO tuple
                    var listItems = list();
                    IsNext(PythonTokens.next, out Token _, PythonTokens.space);
                    RequireNext(PythonTokens.rlist, PythonTokens.space);
                    return new ListExpression(listItems);
                default:
                    throw new NotImplementedException();
            }
        }

        private Token AnyNext(params PythonTokens[] ignore)
        {
            return Dequeue();
        }

        private List<IExpression> list()
        {
            if (IsNext(PythonTokens.rlist, out Token end, PythonTokens.space))
            {
                return new List<IExpression>();
            }

            List<IExpression> arguments = new List<IExpression>();
            arguments.Add(ParseExpression(PythonTokens.space));

            while (IsNext(PythonTokens.next, out Token next, PythonTokens.space))
            {
                arguments.Add(ParseExpression(PythonTokens.space));
            }

            return arguments;
        }

        private List<IArgument> arguments()
        {
            if (IsNext(PythonTokens.rparen, out Token end, PythonTokens.space))
            {
                return new List<IArgument>();
            }

            List<IArgument> arguments = new List<IArgument>();

            if (IsNext(PythonTokens.name, out Token name, PythonTokens.space))
            {
                if (!IsNext(PythonTokens.set, out Token _, PythonTokens.space))
                {
                    Revert(name);
                    arguments.Add(new OrderedArgument(ParseExpression(PythonTokens.space)));
                }
                else
                {
                    arguments.Add(new NamedArgument(name, ParseExpression(PythonTokens.space)));
                }
            }
            else
            {
                arguments.Add(new OrderedArgument(ParseExpression(PythonTokens.space)));
            }

            while (IsNext(PythonTokens.next, out Token next, PythonTokens.space))
            {
                if (IsNext(PythonTokens.name, out name, PythonTokens.space))
                {
                    if (!IsNext(PythonTokens.set, out Token _, PythonTokens.space))
                    {
                        Revert(name);
                        arguments.Add(new OrderedArgument(ParseExpression(PythonTokens.space)));
                    }
                    else
                    {
                        arguments.Add(new NamedArgument(name, ParseExpression(PythonTokens.space)));
                    }
                }
                else
                {
                    arguments.Add(new OrderedArgument(ParseExpression(PythonTokens.space)));
                }
            }

            RequireNext(PythonTokens.rparen, PythonTokens.space);

            return arguments;
        }

        private IRange range()
        {
            bool skipStart = IsNext(PythonTokens.block, out Token _, PythonTokens.space);
            bool skipEnd = IsNext(PythonTokens.block, out Token _, PythonTokens.space);
            bool skipStep = IsNext(PythonTokens.rlist, out Token _, PythonTokens.space);

            if (skipStart)
                throw new NotImplementedException("List slices are not implemeted");

            IExpression start = ParseExpression(PythonTokens.space);
            skipStart = IsNext(PythonTokens.block, out Token _, PythonTokens.space);
            skipEnd = IsNext(PythonTokens.block, out Token _, PythonTokens.space);
            skipStep = IsNext(PythonTokens.rlist, out Token _, PythonTokens.space);

            if (skipStart && !skipStep)
                throw new NotImplementedException("List slices are not implemeted");

            return new SingleItem(start);
        }

        private IExpression fstring()
        {
            throw new NotImplementedException();
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
