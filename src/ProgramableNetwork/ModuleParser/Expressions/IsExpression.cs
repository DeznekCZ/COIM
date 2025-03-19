using Mafi;
using System;

namespace ProgramableNetwork.Python
{
    public class IsExpression : ABinaryOperatorExpression
    {
        public IsExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            if (right is null)
            {
                return (left is null);
            }
            if (right is Type type && !(left is null))
            {
                return left.GetType().IsAssignableTo(type);
            }
            return false;
        }
    }
}