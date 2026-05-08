using PythonAPI.Expressions;
using PythonAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PythonAPI
{
    public class PyTuple : IExpression
    {
        public string StringValue => throw new System.NotImplementedException("could not evaluate");

        public int IntValue => throw new System.NotImplementedException("could not evaluate");

        public long LongValue => throw new System.NotImplementedException("could not evaluate");

        public bool BooleanValue => throw new System.NotImplementedException("could not evaluate");

        public string Path => throw new System.NotImplementedException();

        public List<IExpression> ExpressionLists { get; }

        public PyTuple(List<IExpression> expressions)
        {
            ExpressionLists = expressions;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            object[] elements = ExpressionLists.Select(v => v.GetValue(context)).ToArray();
            Type[] types = Array.ConvertAll(elements, e => e.GetType());
            bool isValueTuple = types.All(t => t.IsValueType);
            if (elements.Length < 7)
            {
                Type tupleType = Type.GetType($"System.{(isValueTuple ? "ValueTuple" : "Tuple")}`{elements.Length}");
                var specificTupleType = tupleType.MakeGenericType(types);
                return Activator.CreateInstance(specificTupleType, elements);
            }
            else
            {
                throw new NotImplementedException($"Tuple is too long: {elements.Length}");
            }
        }

		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new System.NotImplementedException();
		}

		public Task<object> GetValueAsync(IDictionary<string, object> context) {
			return Task.FromResult(GetValue(context));
		}

        public T Get<T>(int index)
        {
            throw new NotImplementedException();
        }
    }
}