using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class Constructor : IExpression
    {
        private Func<dynamic[], dynamic> value;

        public Constructor(Func<dynamic[], dynamic> value)
        {
            this.value = value;
        }

        public string StringValue => throw new NotImplementedException();

        public int IntValue => throw new NotImplementedException();

        public long LongValue => throw new NotImplementedException();

        public bool BooleanValue => throw new NotImplementedException();

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new InvalidCastException("Cannot be referenced");
        }

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            throw new NotImplementedException();
        }

        public dynamic Invoke(dynamic[] args)
        {
            return value.Invoke(args);
        }
    }
}