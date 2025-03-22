using static UnityEngine.GraphicsBuffer;
using System;

namespace ProgramableNetwork.Python
{
    public class InExpression : ABinaryOperatorExpression
    {
        public InExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__contains__(target: NullCheck(left: true, "Target is None: {0}"), key: right);
        }
    }
}