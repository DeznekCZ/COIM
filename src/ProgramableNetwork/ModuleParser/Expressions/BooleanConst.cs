using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class BooleanConst : ConstantExpression
    {
        public BooleanConst(Token token) : base(typeof(bool))
        {
            this.token = token;
            this.BooleanValue = bool.Parse(token.value);
        }

        private readonly Token token;

        public bool BooleanValue { get; }

        public override dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return BooleanValue;
        }
    }
}