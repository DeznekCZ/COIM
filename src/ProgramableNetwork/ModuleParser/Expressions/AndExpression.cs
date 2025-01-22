using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class AndExpression : IExpression
    {
        private IExpression left;
        private IExpression right;

        public AndExpression(IExpression left, IExpression rigth)
        {
            this.left = left;
            this.right = rigth;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            object left = this.left.GetValue(context);
            if (left is null || !Expressions.__bool__(left))
            {
                return false;
            }
            object right = this.right.GetValue(context);
            if (right is null || !Expressions.__bool__(right))
            {
                return false;
            }
            return true;
        }
    }
}