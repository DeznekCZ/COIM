using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class BitOrExpression : IExpression
    {
        private IExpression left;
        private IExpression right;

        public BitOrExpression(IExpression left, IExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            return Expressions.__or__(left.GetValue(context), right.GetValue(context));
        }
    }
}