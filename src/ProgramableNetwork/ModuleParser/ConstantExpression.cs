using System;

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
    }
}