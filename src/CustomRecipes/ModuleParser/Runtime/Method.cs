using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python {
	internal class Method : IExpression {
		private Func<NamedValue[], object> value;
		private Func<NamedValue[], Task<object>> valueAsync;

		public Method(Func<NamedValue[], object> value, string[] arguments = null) {
			this.value = value;
			Arguments = arguments ?? new string[0];
		}

		public Method(Func<NamedValue[], Task<object>> valueAsync, string[] arguments = null) {
			this.valueAsync = valueAsync;
			Arguments = arguments ?? new string[0];
		}

		public string[] Arguments { get; private set; }

		public string Path => throw new NotImplementedException();

		public object Self { get; set; }

		public Reference<object> GetReference(IDictionary<string, object> context) {
			throw new InvalidCastException("Cannot be referenced");
		}

		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new InvalidCastException("Cannot be referenced");
		}

		public object GetValue(IDictionary<string, object> context) {
			throw new NotImplementedException();
		}

		public Task<object> GetValueAsync(IDictionary<string, object> context) {
			throw new NotImplementedException();
		}

		public object Invoke(NamedValue[] args) {
			try {
				return value.Invoke(args);
			} catch (ReturnException r) {
				return r.Value;
			}
		}
	}
}
