using System;
using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public class InvertExpression : IExpression
    {
        private IExpression expression;

        public string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

        public InvertExpression(IExpression expression)
        {
            this.expression = expression;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            return Expressions.__invert__(NullCheck(expression, context, "Is None: {0}"));
        }

        protected object NullCheck(IExpression expression, IDictionary<string, object> context, string format)
        {
            object value = expression.GetValue(context);
            return value is null ? throw new NullReferenceException(string.Format(format, expression)) : value;
        }
    }
}