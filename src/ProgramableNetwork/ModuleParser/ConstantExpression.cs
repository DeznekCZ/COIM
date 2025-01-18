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

        public abstract string StringValue { get; }
        public abstract int IntValue { get; }
        public abstract long LongValue { get; }
        public abstract bool BooleanValue { get; }

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new InvalidCastException("Can not get reference from constant");
        }

        public abstract dynamic GetValue(IDictionary<string, dynamic> context);
    }
}