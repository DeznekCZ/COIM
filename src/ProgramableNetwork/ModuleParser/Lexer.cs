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
        private readonly PythonTokens[] newLineIgnore = new PythonTokens[] { PythonTokens.space, PythonTokens.newline, PythonTokens.comment };
        private readonly PythonTokens[] defaultIgnore = new PythonTokens[] { PythonTokens.space, PythonTokens.comment };

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
                            tree.Add(ParseAssignment(leftExpression));
                            break;
                        }
                        IsNext(PythonTokens.newline, out Token _, PythonTokens.space);
                        tree.Add(new EvaluateStatement(leftExpression));
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

                    case PythonTokens.returnp:
                        if (IsNext(PythonTokens.newline, out Token _, defaultIgnore))
                        {
                            tree.Add(new ReturnStatement(null));
                        }
                        else
                        {
                            tree.Add(new ReturnStatement(ParseExpression(defaultIgnore)));
                        }
                        break;

                    case PythonTokens.comment:
                    case PythonTokens.semicolon:
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

        private AssignmentStatement ParseAssignment(IExpression qualifiedName)
        {
            return new AssignmentStatement(qualifiedName, ParseExpression(PythonTokens.space));
        }

        private IExpression ParseExpression(params PythonTokens[] ignore)
        {
            return disjunct(ignore);
        }

        private IExpression disjunct(PythonTokens[] ignore = null)
        {
            IExpression con = conjunct(ignore ?? defaultIgnore);
            while (IsNext(PythonTokens.or, out Token _, defaultIgnore))
            {
                con = new OrExpression(con, conjunct());
            }
            return con;
        }

        private IExpression conjunct(PythonTokens[] ignore = null)
        {
            IExpression inv = invert(ignore ?? defaultIgnore);
            while (IsNext(PythonTokens.and, out Token _, defaultIgnore))
            {
                inv = new AndExpression(inv, invert());
            }
            return inv;
        }

        private IExpression invert(PythonTokens[] ignore = null)
        {
            int direction = 0;
            while (IsNext(PythonTokens.not, out Token _, ignore ?? defaultIgnore))
            {
                direction++;
            }
            IExpression expression = compare(direction == 0 ? ignore ?? defaultIgnore : defaultIgnore);
            for (int i = 0; i < direction; i++)
            {
                expression = new NotExpression(expression);
            }
            return expression;
        }

        private IExpression compare(PythonTokens[] ignore = null)
        {
            IExpression bitvise = bitviseor(ignore ?? defaultIgnore);
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
            }, out Token operat, defaultIgnore))
            {
                if (operat.type == PythonTokens.not)
                {
                    RequireNext(PythonTokens.ink, defaultIgnore);
                    bitvise = new NotExpression(new InExpression(bitvise, bitviseor()));
                }
                else if (operat.type == PythonTokens.isp)
                {
                    if (IsNext(PythonTokens.not, out Token _, defaultIgnore))
                    {
                        bitvise = new NotExpression(new IsExpression(bitvise, bitviseor()));
                    }
                    else
                    {
                        bitvise = new IsExpression(bitvise, bitviseor());
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

        private IExpression bitviseor(PythonTokens[] ignore = null)
        {
            List<IExpression> ors = new List<IExpression>
            {
                bitvisexor(ignore ?? defaultIgnore)
            };
            while (IsNext(PythonTokens.bitor, out Token _, defaultIgnore))
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

        private IExpression bitvisexor(PythonTokens[] ignore = null)
        {
            List<IExpression> ands = new List<IExpression>
            {
                bitviseand(ignore ?? defaultIgnore)
            };
            while (IsNext(PythonTokens.bitxor, out Token _, defaultIgnore))
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

        private IExpression bitviseand(PythonTokens[] ignore = null)
        {
            List<IExpression> shifts = new List<IExpression>
            {
                bitviseshift(ignore ?? defaultIgnore)
            };
            while (IsNext(PythonTokens.bitand, out Token _, defaultIgnore))
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

        private IExpression bitviseshift(PythonTokens[] ignore = null)
        {
            List<(PythonTokens, IExpression) > shifts = new List<(PythonTokens, IExpression)>
            {
                (0, sum(ignore ?? defaultIgnore))
            };
            while (IsNextOf(new PythonTokens[]{ PythonTokens.shiftl, PythonTokens.shiftr }, out Token shift, defaultIgnore))
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
                    f = new ShiftRightExpression(shifts[i].Item2, f);
                }
            }
            return f;
        }

        private IExpression sum(PythonTokens[] ignore = null)
        {
            List<(PythonTokens, IExpression)> sums = new List<(PythonTokens, IExpression)>
            {
                (0, term(ignore ?? defaultIgnore))
            };
            while (IsNextOf(new PythonTokens[] { PythonTokens.plus, PythonTokens.minus }, out Token shift, defaultIgnore))
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
                    f = new AddExpression(sums[i].Item2, f);
                }
                else
                {
                    f = new ModExpression(sums[i].Item2, f);
                }
            }
            return f;
        }

        private IExpression term(PythonTokens[] ignore = null)
        {
            List<(PythonTokens, IExpression)> sums = new List<(PythonTokens, IExpression)>
            {
                (0, factor(ignore ?? defaultIgnore))
            };
            while (IsNextOf(new PythonTokens[] { PythonTokens.mul, PythonTokens.div, PythonTokens.divint , PythonTokens.mod }, out Token operToken, defaultIgnore))
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

        private IExpression factor(PythonTokens[] ignore = null)
        {
            Stack<PythonTokens> operators = new Stack<PythonTokens>();
            while (IsNextOf(new PythonTokens[] {
                        PythonTokens.plus,
                        PythonTokens.minus,
                        PythonTokens.invert
            }, out Token token, ignore ?? defaultIgnore)) {
                operators.Push(PythonTokens.plus);
            }
            IExpression expression = power(operators.Count == 0 ? ignore ?? defaultIgnore : defaultIgnore);
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

        private IExpression power(PythonTokens[] ignore = null)
        {
            IExpression expression = primary(ignore ?? defaultIgnore);
            while (IsNext(PythonTokens.power, out Token _, defaultIgnore))
            {
                expression = new PowerExpression(expression, factor());
            }
            return expression;
        }

        private IExpression primary(PythonTokens[] ignore = null)
        {
            IExpression expression = atom(ignore ?? defaultIgnore);
            while(IsNextOf(new PythonTokens[]
            {
                PythonTokens.dot,
                PythonTokens.lparen,
                PythonTokens.llist,
            }, out Token member, defaultIgnore))
            {
                if (member.type == PythonTokens.dot)
                {
                    var nameToken = RequireNext(PythonTokens.name, defaultIgnore);
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

        private IExpression atom(PythonTokens[] ignore = null)
        {
            Token decide = AnyNext(ignore ?? defaultIgnore);
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
                    IsNext(PythonTokens.next, out Token _, PythonTokens.space, PythonTokens.newline);
                    RequireNext(PythonTokens.rlist, PythonTokens.space, PythonTokens.newline);
                    return new ListExpression(listItems);
                default:
                    throw new NotImplementedException();
            }
        }

        private Token AnyNext(params PythonTokens[] ignore)
        {
            while (IsNextOf(ignore, out Token _))
                continue;
            return Dequeue();
        }

        private List<IExpression> list()
        {
            if (IsNext(PythonTokens.rlist, out Token end, PythonTokens.space, PythonTokens.newline))
            {
                return new List<IExpression>();
            }

            List<IExpression> arguments = new List<IExpression>();
            arguments.Add(ParseExpression(PythonTokens.space, PythonTokens.newline));

            while (IsNext(PythonTokens.next, out Token next, PythonTokens.space, PythonTokens.newline))
            {
                arguments.Add(ParseExpression(PythonTokens.space, PythonTokens.newline));
            }

            return arguments;
        }

        private List<IArgument> arguments()
        {
            if (IsNext(PythonTokens.rparen, out Token end, newLineIgnore))
            {
                return new List<IArgument>();
            }

            List<IArgument> arguments = new List<IArgument>();

            if (IsNext(PythonTokens.name, out Token name, newLineIgnore))
            {
                if (!IsNext(PythonTokens.set, out Token _, newLineIgnore))
                {
                    Revert(name);
                    arguments.Add(new OrderedArgument(ParseExpression(newLineIgnore)));
                }
                else
                {
                    arguments.Add(new NamedArgument(name, ParseExpression(newLineIgnore)));
                }
            }
            else
            {
                arguments.Add(new OrderedArgument(ParseExpression(newLineIgnore)));
            }

            while (IsNext(PythonTokens.next, out Token next, newLineIgnore))
            {
                if (IsNext(PythonTokens.name, out name, newLineIgnore))
                {
                    if (!IsNext(PythonTokens.set, out Token _, newLineIgnore))
                    {
                        Revert(name);
                        arguments.Add(new OrderedArgument(ParseExpression(newLineIgnore)));
                    }
                    else
                    {
                        arguments.Add(new NamedArgument(name, ParseExpression(newLineIgnore)));
                    }
                }
                else
                {
                    arguments.Add(new OrderedArgument(ParseExpression(newLineIgnore)));
                }
            }

            RequireNext(PythonTokens.rparen, newLineIgnore);

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

        private IExpression ParseQualifiedName(params PythonTokens[] ignore)
        {
            return primary();
        }

        private Token Dequeue()
        {
            Token token = enumerator.First();
            enumerator.RemoveFirst();
            return token;
        }
    }
}
