using PythonAPI;
using PythonAPI.Range;
using System;
using System.Collections.Generic;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions
{
    public class NoneConst : SyncExpression, IExpression
    {
        private Token none;
        public override string Path => throw new NotImplementedException($"Cannot get path from constant {GetType()}");

        public NoneConst(Token none)
        {
            this.none = none;
        }

        public override Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NullReferenceException("null can not be referenced");
        }

        public override object GetValue(IDictionary<string, dynamic> context)
        {
            return null;
        }
    }
}