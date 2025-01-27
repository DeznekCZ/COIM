using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class NoneConst : IExpression
    {
        private Token none;
        public string Path => throw new NotImplementedException($"Cannot get path from constant {GetType()}");

        public NoneConst(Token none)
        {
            this.none = none;
        }

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.NullReferenceException("null can not be referenced");
        }

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return null;
        }
    }
}