using PythonAPI.Range;
using System;
using System.Collections.Generic;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions
{
    public class ObjectConstant : SyncExpression, IExpression
    {
        private object value;

        public ObjectConstant(object value)
        {
            this.value = value;
        }

        public override string Path => throw new NotImplementedException();

        public override Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(IDictionary<string, object> context)
        {
            return value;
        }
    }
}
