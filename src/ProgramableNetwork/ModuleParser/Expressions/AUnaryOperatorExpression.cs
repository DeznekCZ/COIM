using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public abstract class AUnaryOperatorExpression : IExpression
    {
        protected readonly IExpression expression;
        private object expressionValue;

        protected AUnaryOperatorExpression(IExpression expression)
        {
            this.expression = expression;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            expressionValue = this.expression.GetValue(context);
            return Evaluate(expression);
        }

        protected abstract object Evaluate(object value);

        protected object NullCheck(string format)
        {
            return expressionValue is null ? throw new NullReferenceException(string.Format(format, this.expression)) : expressionValue;
        }

        protected object ZeroCheck(bool left, string format)
        {
            return Expressions.__int__(expressionValue) == 0 ? throw new ArgumentException(string.Format(format, this.expression)) : expressionValue;
        }
    }
}