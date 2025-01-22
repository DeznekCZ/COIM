using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public interface IExpression
    {
        Reference<object> GetReference(IDictionary<string, object> context);
        object GetValue(IDictionary<string, object> context);
    }
}