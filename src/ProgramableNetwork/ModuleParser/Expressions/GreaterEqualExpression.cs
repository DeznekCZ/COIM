using System.Collections.Generic;
using UnityEngine;

namespace ProgramableNetwork.Python
{
    public class GreaterEqualExpression : ABinaryOperatorExpression, IComparison
    {
        public GreaterEqualExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        public IExpression Left => left;

        public IExpression Right => right;

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__ge__(left, right);
        }
    }
}