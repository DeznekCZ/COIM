using Mafi;
using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public class DivIntExpression : ABinaryOperatorExpression
    {
        public DivIntExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__divint__(left, ZeroCheck(left: false, "Can not divide by zero: {0}"));
        }
    }
}