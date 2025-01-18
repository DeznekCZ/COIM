using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class StringConstant : ConstantExpression
    {
        private readonly Token token;

        public StringConstant(Token value) : base(typeof(string))
        {
            this.token = value;

            string v = value.value;
            char b = value.value[0];
            v = v.Substring(1, v.Length - 2);
            v = v.Replace($"\\{b}", b.ToString());
            this.Value = v;
        }

        public string Value { get; }

        public override string StringValue => Value;

        public override int IntValue => int.Parse(StringValue);

        public override long LongValue => long.Parse(StringValue);

        public override bool BooleanValue => bool.Parse(StringValue);

        public override dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return Value;
        }
    }
}