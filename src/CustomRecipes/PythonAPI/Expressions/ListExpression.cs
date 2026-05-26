using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions {
	public class ListExpression : IExpression {
		private List<IExpression> listItems;
		public string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

		/// Read-only view of the list's element expressions. Exposed so the recipe-editor
		/// loader can walk `ingredients=[Product(...), ...]` literals at AST level without
		/// having to execute them.
		public IReadOnlyList<IExpression> Items => listItems;

		public ListExpression(List<IExpression> listItems) {
			this.listItems = listItems;
		}

		public Reference<object> GetReference(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}
		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new NotImplementedException();
		}

		public object GetValue(IDictionary<string, object> context) {
			return listItems.Select(v => v.GetValue(context)).ToList();
		}

		public async Task<object> GetValueAsync(IDictionary<string, object> context) {
			return (await Task.WhenAll(
					listItems.Select(v => v.GetValueAsync(context))
				)).ToList();
		}
	}
}
