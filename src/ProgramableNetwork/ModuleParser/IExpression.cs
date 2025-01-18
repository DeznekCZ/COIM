using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public interface IExpression
    {
        string StringValue { get; }
        int IntValue { get; }
        long LongValue { get; }
        bool BooleanValue { get; }

        Reference<dynamic> GetReference(IDictionary<string, dynamic> context);
        dynamic GetValue(IDictionary<string, dynamic> context);
    }
}