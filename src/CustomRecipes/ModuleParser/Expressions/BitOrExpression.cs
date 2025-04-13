using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public class BitOrExpression : ABinaryOperatorExpression
    {
        public BitOrExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__or__(left, right);
        }
    }
}