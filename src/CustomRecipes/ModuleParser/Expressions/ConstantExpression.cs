using System;
using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public abstract class ConstantExpression : IExpression
    {
        private Type type;
        public string Path => throw new NotImplementedException($"Cannot get path from constant {GetType()}");

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