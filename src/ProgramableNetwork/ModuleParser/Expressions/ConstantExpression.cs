using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public abstract class ConstantExpression : IExpression
    {
        private Type type;

        public ConstantExpression(Type type)
        {
            this.type = type;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new InvalidCastException("Can not get reference from constant");
        }

        public abstract object GetValue(IDictionary<string, object> context);
    }
}