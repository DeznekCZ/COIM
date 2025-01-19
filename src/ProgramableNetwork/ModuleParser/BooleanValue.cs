using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class BooleanConst : ConstantExpression
    {
        public BooleanConst(Token token) : base(typeof(bool))
        {
            this.token = token;
            this.StringValue = token.value.ToLower();
            this.BooleanValue = bool.Parse(StringValue);
        }

        private readonly Token token;

        public override string StringValue { get; }
        public override int IntValue => BooleanValue ? 1 : 0;
        public override bool BooleanValue { get; }

        public override long LongValue => throw new NotImplementedException();

        public override dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return BooleanValue;
        }
    }
}