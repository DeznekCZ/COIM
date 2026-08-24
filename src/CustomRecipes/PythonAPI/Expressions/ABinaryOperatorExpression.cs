using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions {
	public abstract class ABinaryOperatorExpression : IExpression {
		protected readonly IExpression left;
		protected readonly IExpression right;
		private object leftValue;
		private object rightValue;

		public IExpression Left => left;
		public IExpression Right => right;
		public virtual string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

		protected ABinaryOperatorExpression(IExpression left, IExpression right) {
			this.left = left;
			this.right = right;
		}

		// Most operators produce a value that cannot be assigned back to. IndexExpression
		// overrides both: `x[k] = v` needs a writable reference to the subscripted slot.
		public virtual Reference<object> GetReference(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}
		public virtual Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new NotImplementedException();
		}

		public object GetValue(IDictionary<string, object> context) {
			leftValue = this.left.GetValue(context);
			rightValue = this.right.GetValue(context);
			return Evaluate(leftValue, rightValue);
		}
		public async Task<object> GetValueAsync(IDictionary<string, object> context) {
			leftValue = await this.left.GetValueAsync(context);
			rightValue = await this.right.GetValueAsync(context);
			return Evaluate(leftValue, rightValue);
		}

		protected abstract object Evaluate(object left, object right);

		protected object NullCheck(bool left, string format) {
			if (left) {
				return leftValue is null
					? throw new NullReferenceException(string.Format(format, this.left))
					: leftValue;
			} else {
				return rightValue is null ? throw new NullReferenceException(string.Format(format, right)) : rightValue;
			}
		}

		protected object ZeroCheck(bool left, string format) {
			if (left) {
				return Expressions.__int__(leftValue) == 0
					? throw new ArgumentException(string.Format(format, this.left))
					: leftValue;
			} else {
				return Expressions.__int__(leftValue) == 0
					? throw new ArgumentException(string.Format(format, right))
					: rightValue;
			}
		}
	}
}
