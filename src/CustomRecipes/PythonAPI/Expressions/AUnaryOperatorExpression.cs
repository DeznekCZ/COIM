using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions {
	public abstract class AUnaryOperatorExpression : IExpression {
		protected readonly IExpression expression;
		private object expressionValue;

		public virtual string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

		protected AUnaryOperatorExpression(IExpression expression) {
			this.expression = expression;
		}

		public Reference<object> GetReference(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}
		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new NotImplementedException();
		}

		public object GetValue(IDictionary<string, object> context) {
			expressionValue = this.expression.GetValue(context);
			return Evaluate(expressionValue);
		}

		public async Task<object> GetValueAsync(IDictionary<string, object> context) {
			expressionValue = await this.expression.GetValueAsync(context);
			return Evaluate(expressionValue);
		}

		protected abstract object Evaluate(object value);

		protected object NullCheck(string format) {
			return expressionValue is null
				? throw new NullReferenceException(string.Format(format, this.expression))
				: expressionValue;
		}

		protected object ZeroCheck(bool left, string format) {
			return Expressions.__int__(expressionValue) == 0
				? throw new ArgumentException(string.Format(format, this.expression))
				: expressionValue;
		}
	}
}
