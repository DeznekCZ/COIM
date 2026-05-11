using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    /// <summary>
    /// Following grammar, but cutted:
    /// <br/>https://docs.python.org/3/reference/grammar.html
    /// </summary>
    public partial class Lexer
    {
        private LinkedList<Token> enumerator;
        private readonly PythonTokens[] newLineIgnore = new PythonTokens[] { PythonTokens.newline, PythonTokens.indent, PythonTokens.dedent, PythonTokens.comment };
        private readonly PythonTokens[] defaultIgnore = new PythonTokens[] { PythonTokens.comment };

        public static Block Parse(Token[] tokens)
        {
            if (tokens == null || tokens.Length == 0) {
				return new Block();
			}

			Lexer lexer = new Lexer();
            lexer.enumerator = new LinkedList<Token>(tokens);
            return lexer.ParseTree();
        }

        private Block ParseTree()
        {
            return ParseBlock(null, PythonTokens.eof);
        }

        private Block ParseBlock(Block parentTree, PythonTokens blockEnd)
        {
            Block tree = new Block(parentTree);
            while (enumerator.Count > 0 && enumerator.First.Value.type != blockEnd)
            {
                Token token = Dequeue();
                switch (token.type)
                {
                    case PythonTokens.from:
                        IExpression p = primary();
                        RequireNext(PythonTokens.import);
                        // Python supports both the bare list `from x import a, b`
                        // and the parenthesised form `from x import (a, b,)`
                        // — the latter allows splitting the list across lines
                        // (the tokenizer's parenDepth already suppresses
                        // newline/indent/dedent inside `( ... )`) and a
                        // trailing comma, neither of which the bare form
                        // accepts.  We branch on `(` and parse the parenthesised
                        // body manually so we can stop on a closing `)` and
                        // tolerate the trailing comma without retrofitting
                        // NextList for both behaviours.
                        List<Token> importNames;
                        if (IsNext(PythonTokens.lparen, out Token _, defaultIgnore))
                        {
                            importNames = new List<Token>();
                            importNames.Add(RequireNext(PythonTokens.name, newLineIgnore));
                            bool consumedRparen = false;
                            while (IsNext(PythonTokens.next, out Token _, newLineIgnore))
                            {
                                // Trailing comma → next is `)`; consume it
                                // and stop.  Without this branch the rparen
                                // would fall through to the RequireNext(name)
                                // below and raise a parse error.
                                if (IsNext(PythonTokens.rparen, out Token _, newLineIgnore))
                                {
                                    consumedRparen = true;
                                    break;
                                }
                                importNames.Add(RequireNext(PythonTokens.name, newLineIgnore));
                            }
                            if (!consumedRparen)
                            {
                                RequireNext(PythonTokens.rparen, newLineIgnore);
                            }
                        }
                        else
                        {
                            importNames = NextList(PythonTokens.name, next: PythonTokens.next);
                        }
                        ImportStatement import = new ImportStatement(p, importNames);
                        tree.Add(import);
                        RequireNext(PythonTokens.newline);
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

                    case PythonTokens.forp:
                        ParseFor(tree);
                        break;

                    case PythonTokens.whilep:
                        ParseWhile(tree);
                        break;

                    case PythonTokens.breakp:
                        // `break` is a single statement.  Don't consume the newline
                        // unconditionally — the outer while-loop expects the
                        // dispatcher to leave terminators for the `case newline`
                        // branch to handle.
                        IsNext(PythonTokens.newline, out Token _, defaultIgnore);
                        tree.Add(new BreakStatement());
                        break;

                    case PythonTokens.continuep:
                        IsNext(PythonTokens.newline, out Token _, defaultIgnore);
                        tree.Add(new ContinueStatement());
                        break;

                    case PythonTokens.elif:
                        if (!(tree.statements.Last() is IfStatement ifs1)) {
							throw new PythonParseException(token, $"elif is allowed only after if or elif statement");
						}

						tree.statements.RemoveAt(tree.statements.Count - 1);
                        ParseElIf(ifs1, tree);
                        break;

                    case PythonTokens.elsep:
                        if (!(tree.statements.Last() is IfStatement ifs2)) {
							throw new PythonParseException(token, $"else is allowed only after if or elif statement");
						}

						tree.statements.RemoveAt(tree.statements.Count - 1);
                        ParseElse(ifs2, tree);
                        break;

                    case PythonTokens.name:
                        // PLC-PY section header: `init:` and `main:` only at
                        // the script's TOP level (parentTree is null).  Same
                        // shape as ParseIf / ParseFor / ParseWhile — a name
                        // token followed by `:` introduces an indented body.
                        // Restricting to top level keeps `init = 1` / `main
                        // = foo()` working as ordinary assignments anywhere
                        // a section can't legally appear (inside a def, an
                        // if branch, a for body, etc.).
                        if (parentTree == null
                            && (token.value == "init" || token.value == "main")
                            && enumerator.Count > 0
                            && enumerator.First.Value.type == PythonTokens.block)
                        {
                            ParseSection(tree, token.value);
                            break;
                        }
                        // Assignment OR bare expression-as-statement.
                        Revert(token);
                        IExpression leftExpression = ParseExpression();
                        // One of seven assignment-like operators may follow:
                        // `=` (plain) or one of the six augmented forms
                        // (`+=`, `-=`, `*=`, `/=`, `<<=`, `>>=`).  The
                        // augmented forms desugar to `target = target OP rhs`
                        // — building the AST node here keeps AssignmentStatement
                        // simple (one shape, one execute path) and lets the
                        // runtime stay oblivious to the compound forms.
                        if (IsNextOf(new PythonTokens[] {
                            PythonTokens.set,
                            PythonTokens.setplus,
                            PythonTokens.setminus,
                            PythonTokens.setmul,
                            PythonTokens.setdiv,
                            PythonTokens.setshl,
                            PythonTokens.setshr,
                        }, out Token assignOp, defaultIgnore))
                        {
                            IExpression rhs = ParseExpression();
                            IExpression assignedValue;
                            switch (assignOp.type)
                            {
                                case PythonTokens.set:
                                    assignedValue = rhs;
                                    break;
                                case PythonTokens.setplus:
                                    assignedValue = new AddExpression(leftExpression, rhs);
                                    break;
                                case PythonTokens.setminus:
                                    // Mirrors the sum() rule: `a - b` is
                                    // built as `a + (-b)` so the AST stays
                                    // a single AddExpression family instead
                                    // of needing a dedicated SubExpression.
                                    assignedValue = new AddExpression(leftExpression, new NegativeExpression(rhs));
                                    break;
                                case PythonTokens.setmul:
                                    assignedValue = new MulExpression(leftExpression, rhs);
                                    break;
                                case PythonTokens.setdiv:
                                    assignedValue = new DivExpression(leftExpression, rhs);
                                    break;
                                case PythonTokens.setshl:
                                    assignedValue = new ShiftLeftExpression(leftExpression, rhs);
                                    break;
                                case PythonTokens.setshr:
                                    assignedValue = new ShiftRightExpression(leftExpression, rhs);
                                    break;
                                default:
                                    throw new PythonParseException(assignOp, "<unreachable: unhandled assignment operator>");
                            }
                            tree.Add(new AssignmentStatement(leftExpression, assignedValue));
                        }
                        else
                        {
                            // Bare expression-as-statement (e.g. `foo()`).
                            // Only emit when there's no assignment so a
                            // simple `x = 5` doesn't redundantly re-evaluate
                            // the LHS after the AssignmentStatement runs.
                            tree.Add(new EvaluateStatement(leftExpression));
                        }
                        IsNext(PythonTokens.newline, out Token _);
                        break;

                    case PythonTokens.newline:
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
                    case PythonTokens.pass:
                        break;

                    case PythonTokens.undefined:
                    default:
                        throw new PythonParseException(token, $"unrecognized or unimplemented token");
                }
            }
            return tree;
        }

        private void Revert(Token token)
        {
            enumerator.AddFirst(token);
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
                    // RHS descends to bitviseor — the same level used for the
                    // LHS at the top of compare().  Was previously hopping
                    // straight to bitvisexor, which skipped the `|` level
                    // and made `a == b | c` parse as `(a == b) | c` instead
                    // of Python's `a == (b | c)` (comparisons are LOWER
                    // precedence than bitwise-or per CPython grammar).
                    switch (operat.type)
                    {
                        case PythonTokens.eq:
                            bitvise = new EqualExpression(bitvise, bitviseor());
                            break;
                        case PythonTokens.neq:
                            bitvise = new NotExpression(new EqualExpression(bitvise, bitviseor()));
                            break;
                        case PythonTokens.lre:
                            bitvise = new LowerEqualExpression(bitvise, bitviseor());
                            break;
                        case PythonTokens.gre:
                            bitvise = new GreaterEqualExpression(bitvise, bitviseor());
                            break;
                        case PythonTokens.lr:
                            bitvise = new LowerExpression(bitvise, bitviseor());
                            break;
                        case PythonTokens.gr:
                            bitvise = new GreaterExpression(bitvise, bitviseor());
                            break;
                        default:
                            break;
                    }
                }
            }
            if (bitvise is IComparison cRight && cRight.Left is IComparison cLeft)
            {
                bitvise = CompareRangeExpression.CreateFrom(
                    left: cLeft.Left,
                    leftOperator: cLeft.GetType(),
                    middle: cLeft.Right,
                    rightOperator: cRight.GetType(),
                    right: cRight.Right
                );
            }
            return bitvise;
        }

        // Left-associative chain: `a | b | c` ⇒ `(a | b) | c`.
        // Each binary level used to collect all operands into a list and
        // then fold right-to-left, which produced right-associative ASTs.
        // For commutative+associative ops (or/xor/and) the value was the
        // same but for non-commutative ops (sub/div/shift) the result was
        // wrong (e.g. `1 - 2 + 3` evaluated as `1 - (2 + 3) = -4`).  Folding
        // left-to-right inline matches Python's grammar for all binary
        // levels and keeps the AST shape consistent across operators.
        private IExpression bitviseor(PythonTokens[] ignore = null)
        {
            IExpression f = bitvisexor(ignore ?? defaultIgnore);
            while (IsNext(PythonTokens.bitor, out Token _, defaultIgnore))
            {
                f = new BitOrExpression(f, bitvisexor());
            }
            return f;
        }

        private IExpression bitvisexor(PythonTokens[] ignore = null)
        {
            IExpression f = bitviseand(ignore ?? defaultIgnore);
            while (IsNext(PythonTokens.bitxor, out Token _, defaultIgnore))
            {
                f = new BitXorExpression(f, bitviseand());
            }
            return f;
        }

        private IExpression bitviseand(PythonTokens[] ignore = null)
        {
            IExpression f = bitviseshift(ignore ?? defaultIgnore);
            while (IsNext(PythonTokens.bitand, out Token _, defaultIgnore))
            {
                f = new BitAndExpression(f, bitviseshift());
            }
            return f;
        }

        private IExpression bitviseshift(PythonTokens[] ignore = null)
        {
            IExpression f = sum(ignore ?? defaultIgnore);
            while (IsNextOf(new PythonTokens[] { PythonTokens.shiftl, PythonTokens.shiftr }, out Token shift, defaultIgnore))
            {
                IExpression rhs = sum();
                if (shift.type == PythonTokens.shiftl)
                {
                    f = new ShiftLeftExpression(f, rhs);
                }
                else
                {
                    f = new ShiftRightExpression(f, rhs);
                }
            }
            return f;
        }

        private IExpression sum(PythonTokens[] ignore = null)
        {
            IExpression f = term(ignore ?? defaultIgnore);
            while (IsNextOf(new PythonTokens[] { PythonTokens.plus, PythonTokens.minus }, out Token op, defaultIgnore))
            {
                IExpression rhs = term();
                if (op.type == PythonTokens.plus)
                {
                    f = new AddExpression(f, rhs);
                }
                else
                {
                    // Subtraction: a - b ≡ a + (-b).  The right-hand operand
                    // is the term we just parsed — wrapping it in a Negative
                    // here keeps the AST a clean sequence of AddExpressions
                    // while preserving Python's left-associative semantics
                    // (`a - b + c` ⇒ `(a + (-b)) + c`, not `a - (b + c)`).
                    f = new AddExpression(f, new NegativeExpression(rhs));
                }
            }
            return f;
        }

        private IExpression term(PythonTokens[] ignore = null)
        {
            IExpression f = factor(ignore ?? defaultIgnore);
            while (IsNextOf(new PythonTokens[] { PythonTokens.mul, PythonTokens.div, PythonTokens.divint, PythonTokens.mod }, out Token op, defaultIgnore))
            {
                IExpression rhs = factor();
                if (op.type == PythonTokens.mul)
                {
                    f = new MulExpression(f, rhs);
                }
                else if (op.type == PythonTokens.div)
                {
                    f = new DivExpression(f, rhs);
                }
                else if (op.type == PythonTokens.divint)
                {
                    f = new DivIntExpression(f, rhs);
                }
                else
                {
                    f = new ModExpression(f, rhs);
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
                // Push the actual operator we matched — was previously hard-coded
                // to `plus`, which made unary `-x` and `~x` silently behave as
                // identity (no-op).  The pop loop below already dispatches per
                // token, so just feed it the right one.
                operators.Push(token.type);
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
                    default: throw new PythonParseException(Token.EOF, "<unreachable error>");
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
                else if (member.type == PythonTokens.llist)
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
                    return fstring(decide);
                case PythonTokens.lparen:
                    // TODO tuple
                    IExpression expression = ParseExpression(newLineIgnore);
                    RequireNext(PythonTokens.rparen);
                    return expression;
                case PythonTokens.llist:
                    // TODO tuple
                    var listItems = list();
                    IsNext(PythonTokens.next, out Token _, PythonTokens.newline);
                    RequireNext(PythonTokens.rlist, newLineIgnore);
                    return new ListExpression(listItems);
                default:
                    Revert(decide);
                    throw new PythonParseException(decide, $"unexpected or not implemented atom token");
            }
        }

        private Token AnyNext(params PythonTokens[] ignore)
        {
            while (IsNextOf(ignore, out Token _)) {
				continue;
			}
			return Dequeue();
        }

        private List<IExpression> list()
        {
            if (IsNext(PythonTokens.rlist, out Token end, newLineIgnore))
            {
                return new List<IExpression>();
            }

            List<IExpression> arguments = new List<IExpression>();
            arguments.Add(ParseExpression(newLineIgnore));

            while (IsNext(PythonTokens.next, out Token next, newLineIgnore))
            {
                arguments.Add(ParseExpression(newLineIgnore));
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
            bool skipStart = IsNext(PythonTokens.block, out Token _);
            bool skipEnd = IsNext(PythonTokens.block, out Token _);
            bool skipStep = IsNext(PythonTokens.rlist, out Token _);

            if (skipStart) {
				throw new PythonParseException(AnyNext(), "List slices are not implemeted");
			}

			IExpression start = ParseExpression();
            skipStart = IsNext(PythonTokens.block, out Token _);
            skipEnd = IsNext(PythonTokens.block, out Token _);
            skipStep = IsNext(PythonTokens.rlist, out Token _);

            if (skipStart && !skipStep) {
				throw new PythonParseException(AnyNext(), "List slices are not implemeted");
			}

			return new SingleItem(start);
        }

        private IExpression fstring(Token begin)
        {
            if (begin.value.EndsWith("\"") || begin.value.EndsWith("'"))
            {
                Token stringToken = new Token(begin.source, begin.lineText, begin.line, begin.column + 1, begin.length - 1, PythonTokens.str, begin.value.Substring(1));
                return new StringConstant(stringToken);
            }

            char endingCharacter = begin.value[1];
            // clear unrequired f on beginning
            begin = new Token(begin.source, begin.lineText, begin.line, begin.column + 1, begin.length - 1, PythonTokens.str, begin.value.Substring(1));

            List<IExpression> value = new List<IExpression>();
            string ReplaceEnds(string s) => $"{endingCharacter}{s.Substring(1, s.Length - 2)}{endingCharacter}";

            while (true) // loop trough next tokens
            {
                if (begin.value.Length > 2)
                {
                    Token stringToken = new Token(begin.source, begin.lineText, begin.line, begin.column + 1, begin.length - 1, PythonTokens.str,
                        ReplaceEnds(begin.value.Replace("{{", "{")
                                               .Replace("}}", "}"))
                    );
                    value.Add(new StringConstant(stringToken));
                }
                if (begin.value.EndsWith("{"))
                {
                    value.Add(new CallExpression(
                        new ObjectConstant(new Constructor((args) => Expressions.__str__(args[0].Value))),
                        new List<IArgument> { new OrderedArgument(ParseExpression()) }
                    ));
                    begin = RequireNext(PythonTokens.fstrmiddle);
                    continue;
                }
                return new CallExpression(
                    new ObjectConstant(new Constructor((args) => string.Concat(args.Select(a => (string)a.Value)))),
                    value.Select(e => new OrderedArgument(e)).ToList<IArgument>()
                );
            }
        }

        private List<Token> NextList(PythonTokens type, PythonTokens next, params PythonTokens[] ignore)
        {
            List<Token> list = new List<Token>();

            Token first = RequireNext(type, ignore);
            list.Add(first);

            while(IsNext(next, out Token _, ignore)) {
				list.Add(RequireNext(type, ignore));
			}

			return list;
        }

        private List<Token> NextList0(PythonTokens type, PythonTokens next, params PythonTokens[] ignore)
        {
            List<Token> list = new List<Token>();

            if (!IsNext(type, out Token first, ignore)) {
				return list;
			}

			list.Add(first);

            while(IsNext(next, out Token _, ignore)) {
				list.Add(RequireNext(type, ignore));
			}

			return list;
        }

        private Token RequireNext(PythonTokens type, params PythonTokens[] ignore)
        {
            var token = Dequeue();
            while (ignore.Contains(token.type)) {
				token = Dequeue();
			}
			if (token.type != type) {
				throw new PythonParseException(token, $"Expected token '{type}', got: {token}");
			}
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

                while (stack.Count > 0) {
					enumerator.AddFirst(stack.Pop());
				}

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

                while (stack.Count > 0) {
					enumerator.AddFirst(stack.Pop());
				}

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
