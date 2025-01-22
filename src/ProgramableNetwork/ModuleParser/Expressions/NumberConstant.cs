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

        public NumberConstant(int v) : base(typeof(Fix32))
        {
            this.token = new Token(new System.IO.FileInfo("f"), 0, 0, 1, PythonTokens.number, v.ToString());
            this.Value = v.ToFix32();
        }

        private readonly Token token;
        private int v;

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