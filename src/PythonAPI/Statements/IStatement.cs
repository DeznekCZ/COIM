using System.Collections.Generic;
using System.Threading.Tasks;

namespace PythonAPI.Statements
{
    public interface IStatement
    {
        void Execute(IDictionary<string, object> context);
		Task ExecuteAsync(IDictionary<string, object> context);
	}
}