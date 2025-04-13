using Mafi;
using System.Collections.Generic;

namespace CustomRecipes.Python
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