using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class NegativeExpression : AUnaryOperatorExpression
    {
        public NegativeExpression(IExpression expression) : base(expression)
        {
        }

        protected override object Evaluate(object value)
        {
            return Expressions.__neg__(NullCheck("Cannot negate None: {0}"));
        }
    }
}