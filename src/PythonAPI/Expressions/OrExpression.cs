using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions {
	public class OrExpression : IExpression {
		private IExpression left;
		private IExpression right;
		public string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

		public OrExpression(IExpression left, IExpression right) {
			this.left = left;
			this.right = right;
		}

		public Reference<object> GetReference(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}
		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new NotImplementedException();
		}

		public object GetValue(IDictionary<string, object> context) {
			object left = this.left.GetValue(context);
			if (left is null || !Expressions.__bool__(left)) {
				object right = this.right.GetValue(context);
				if (right is null || !Expressions.__bool__(right)) {
					return false;
				}
			}
			return true;
		}
		public async Task<object> GetValueAsync(IDictionary<string, object> context) {
			object left = await this.left.GetValueAsync(context);
			if (left is null || !Expressions.__bool__(left)) {
				object right = await this.right.GetValueAsync(context);
				if (right is null || !Expressions.__bool__(right)) {
					return false;
				}
			}
			return true;
		}
	}
}
