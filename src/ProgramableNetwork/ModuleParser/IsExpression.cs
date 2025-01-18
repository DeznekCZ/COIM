using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class IsExpression : IExpression
    {
        private IExpression leftValue;
        private bool not;
        private IExpression rightValue;

        public IsExpression(IExpression finalExpression, bool not, IExpression qualifiedName)
        {
            this.leftValue = finalExpression;
            this.not = not;
            this.rightValue = qualifiedName;
        }

        public string StringValue => throw new System.NotImplementedException();

        public int IntValue => throw new System.NotImplementedException();

        public long LongValue => throw new System.NotImplementedException();

        public bool BooleanValue => throw new System.NotImplementedException();

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.InvalidCastException("can not be referenced");
        }

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            dynamic left = leftValue.GetValue(context);
            dynamic right = rightValue.GetValue(context);

            if (right is null)
            {
                return (left is null) != not;
            }
            if (right is Type type && !(left is null))
            {
                return type.IsAssignableFrom(((object)left).GetType()) != not;
            }
            return false;
        }
    }
}