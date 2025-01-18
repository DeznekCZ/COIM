using Mafi;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class NumberConstant : ConstantExpression
    {
        public NumberConstant(Token value) : base(typeof(Fix32))
        {
            this.token = value;
            this.Value = (Fix32) double.Parse(value.value);
        }

        private readonly Token token;

        public Fix32 Value { get; }

        public override string StringValue => Value.ToString();

        public override int IntValue => Value.IntegerPart;

        public override long LongValue => Value.IntegerPart;

        public override bool BooleanValue => Value > 0;

        public override dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return Value;
        }
    }
}