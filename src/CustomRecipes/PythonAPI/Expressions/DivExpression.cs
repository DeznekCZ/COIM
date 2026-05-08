using Mafi;
using System.Collections.Generic;

namespace PythonAPI.Expressions
{
    public class DivExpression : ABinaryOperatorExpression
    {
        public DivExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__div__(left, ZeroCheck(left: false, "Can not divide by zero: {0}"));
        }
    }
}