using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class ObjectConstant : IExpression
    {
        private object value;

        public ObjectConstant(object value)
        {
            this.value = value;
        }

        public string Path => throw new NotImplementedException();

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            return value;
        }
    }
}
