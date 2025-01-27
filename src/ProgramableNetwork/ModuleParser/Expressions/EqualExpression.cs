using Mafi.Core.Factory.Lifts;
using System.Collections.Generic;
using UnityEngine;

namespace ProgramableNetwork.Python
{
    internal class EqualExpression : ABinaryOperatorExpression
    {
        public EqualExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            if (left is null && right is null)
            {
                return true;
            }
            if (left is null || right is null)
            {
                return false;
            }
            return Expressions.__eq__(left, right);
        }
    }
}