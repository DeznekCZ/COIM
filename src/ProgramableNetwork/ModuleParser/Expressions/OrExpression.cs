using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class OrExpression : IExpression
    {
        private IExpression left;
        private IExpression right;
        public string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

        public OrExpression(IExpression left, IExpression right)
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
            object left = this.left.GetValue(context);
            if (left is null || !Expressions.__bool__(left))
            {
                object right = this.right.GetValue(context);
                if (right is null || !Expressions.__bool__(right))
                {
                    return false;
                }
            }
            return true;
        }
    }
}