using System.Collections.Generic;
using UnityEngine;

namespace ProgramableNetwork.Python
{
    internal class GreaterEqualExpression : ABinaryOperatorExpression
    {
        public GreaterEqualExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__ge__(left, right);
        }
    }
}