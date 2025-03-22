using Mafi.Core.Factory.Lifts;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

namespace ProgramableNetwork.Python
{
    public class BitXorExpression : ABinaryOperatorExpression
    {
        public BitXorExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__xor__(left, right);
        }
    }
}