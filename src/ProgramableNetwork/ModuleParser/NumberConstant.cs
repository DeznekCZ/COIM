using Mafi;
using System;

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
    }
}