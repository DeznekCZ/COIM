using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python
{
	public class InvertExpression : IExpression {
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

		public async Task<object> GetValueAsync(IDictionary<string, object> context) {
			return Expressions.__invert__(NullCheck(await expression.GetValueAsync(context), context, "Is None: {0}"));
		}

        public object GetValue(IDictionary<string, object> context)
        {
            return Expressions.__invert__(NullCheck(expression.GetValue(context), context, "Is None: {0}"));
        }

        protected object NullCheck(object value, IDictionary<string, object> context, string format)
        {
            return value is null ? throw new NullReferenceException(string.Format(format, expression)) : value;
        }

		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new NotImplementedException();
		}
	}
}