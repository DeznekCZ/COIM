using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ProgramableNetwork.Python
{
    internal abstract class ExpressionGraph
    {
        private Func<ExpressionGraph, ExpressionGraph> visitorFunction;

        public ExpressionGraph()
        {
        }

        public ExpressionGraph visitor(Func<ExpressionGraph, ExpressionGraph> visitor)
        {
            visitorFunction = visitor;
            return this;
        }

        public virtual IExpression visit()
        {
            if (visitorFunction != null)
                return visitorFunction(this).visit();

            throw new NotImplementedException();
        }

        public static TokenGraph Token(PythonTokens token)
        {
            return new TokenGraph(token);
        }

        public class TokenGraph : ExpressionGraph
        {
            private PythonTokens token;

            public TokenGraph(PythonTokens token)
            {
                this.token = token;
            }
        }

        public static AndVisitor And(ExpressionGraph[] expressions)
        {
            return new AndVisitor(expressions);
        }

        public class AndVisitor : ExpressionGraph
        {
            private ExpressionGraph[] expressions;

            public AndVisitor(ExpressionGraph[] expressions)
            {
                this.expressions = expressions;
            }
        }

        public static OrVisitor Or(ExpressionGraph[] expressions)
        {
            return new OrVisitor(expressions);
        }

        public class OrVisitor : ExpressionGraph
        {
            private ExpressionGraph[] expressions;

            public OrVisitor(ExpressionGraph[] expressions)
            {
                this.expressions = expressions;
            }

            public override IExpression visit()
            {
                foreach (var expression in expressions)
                {
                    try
                    {
                        return expression.visit();
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

                throw new NotImplementedException("No subexpression matched");
            }
        }

        public static MultipleVisitor Multiple(ExpressionGraph exp)
        {
            return new MultipleVisitor(exp);
        }

        public class MultipleVisitor : ExpressionGraph
        {
            private ExpressionGraph exp;
            private List<IExpression> expression;

            public MultipleVisitor(ExpressionGraph exp)
            {
                this.exp = exp;
            }

            public override IExpression visit()
            {
                int count = 0;
                expression = new List<IExpression>();
                while (true)
                {
                    try
                    {
                        expression.Add(exp.visit());
                        count++;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                return visitorFunction(this).visit();
            }
        }

        public static MaybeVisitor Maybe(ExpressionGraph exp)
        {
            return new MaybeVisitor(exp);
        }

        public class MaybeVisitor : ExpressionGraph
        {
            private ExpressionGraph exp;

            public MaybeVisitor(ExpressionGraph exp)
            {
                this.exp = exp;
            }

            public override IExpression visit()
            {
                try
                {
                    return exp.visit();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}