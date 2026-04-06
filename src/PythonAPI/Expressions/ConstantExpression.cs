using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions
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
		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new InvalidCastException("Can not get reference from constant");
		}

		public abstract object GetValue(IDictionary<string, object> context);

		public Task<object> GetValueAsync(IDictionary<string, object> context) {
			return Task.FromResult(GetValue(context));
		}
	}
}