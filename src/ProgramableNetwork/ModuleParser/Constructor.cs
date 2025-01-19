using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class Constructor : IExpression
    {
        private Func<object[], object> value;

        public Constructor(Func<object[], object> value)
        {
            this.value = value;
        }

        public string StringValue => throw new NotImplementedException();

        public int IntValue => throw new NotImplementedException();

        public long LongValue => throw new NotImplementedException();

        public bool BooleanValue => throw new NotImplementedException();

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new InvalidCastException("Cannot be referenced");
        }

        public object GetValue(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public object Invoke(object[] args)
        {
            return value.Invoke(args);
        }
    }
}