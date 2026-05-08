using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions
{
    public class VariableExpression : IExpression
    {
        private string value;

        public VariableExpression(string value)
        {
            this.value = value;
        }

        public string Path => value;

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            return new Reference<object>(
                    (value) => context[this.value] = value,
                    () => context[this.value]
                );
        }
		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			return Task.FromResult(GetReference(context));
		}

		public object GetValue(IDictionary<string, object> context)
        {
            return context.TryGetValue(this.value, out object value) ? value : null;
        }
		public Task<object> GetValueAsync(IDictionary<string, object> context) {
			return Task.FromResult(GetValue(context));
		}
	}
}