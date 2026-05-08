using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions
{
    public interface IExpression
    {
        string Path { get; }
        Reference<object> GetReference(IDictionary<string, object> context);
        Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context);
        object GetValue(IDictionary<string, object> context);
		Task<object> GetValueAsync(IDictionary<string, object> context);
	}
}