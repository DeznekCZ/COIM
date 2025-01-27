using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class PyTuple : IExpression
    {
        public string StringValue => throw new System.NotImplementedException("could not evaluate");

        public int IntValue => throw new System.NotImplementedException("could not evaluate");

        public long LongValue => throw new System.NotImplementedException("could not evaluate");

        public bool BooleanValue => throw new System.NotImplementedException("could not evaluate");

        public string Path => throw new System.NotImplementedException();

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.NotImplementedException();
        }

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            throw new System.NotImplementedException();
        }
    }
}