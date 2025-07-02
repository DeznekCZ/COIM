using Mafi;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class IndexExpression : ABinaryOperatorExpression
    {
        public IndexExpression(IExpression left, IRange right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            if (right is Range range)
                return Expressions.__range__(left, range);
            else
                return Expressions.__getitem__(left, right);
        }

        public override Reference<object> GetReference(IDictionary<string, object> context)
        {
            return new Reference<object>(
                (v) => Expressions.__setitem__(left.GetValue(context), right.GetValue(context), v),
                () => GetValue(context)
            );
        }
    }
}