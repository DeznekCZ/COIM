using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public interface IExpression
    {
        string Path { get; }
        Reference<object> GetReference(IDictionary<string, object> context);
        object GetValue(IDictionary<string, object> context);
    }
}