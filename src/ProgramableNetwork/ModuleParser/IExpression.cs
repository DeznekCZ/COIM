using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public interface IExpression
    {
        string StringValue { get; }
        int IntValue { get; }
        long LongValue { get; }
        bool BooleanValue { get; }

        Reference<object> GetReference(IDictionary<string, object> context);
        object GetValue(IDictionary<string, object> context);
    }
}