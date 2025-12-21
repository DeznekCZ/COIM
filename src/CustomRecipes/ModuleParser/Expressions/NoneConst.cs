using System;
using System.Collections.Generic;

namespace CustomAssets.Python
{
    public class NoneConst : SyncExpression, IExpression
    {
        private Token none;
        public override string Path => throw new NotImplementedException($"Cannot get path from constant {GetType()}");

        public NoneConst(Token none)
        {
            this.none = none;
        }

        public override Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.NullReferenceException("null can not be referenced");
        }

        public override dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return null;
        }
    }
}