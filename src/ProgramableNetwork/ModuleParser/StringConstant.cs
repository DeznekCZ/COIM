using System;

namespace ProgramableNetwork.Python
{
    public class StringConstant : ConstantExpression
    {
        private readonly Token token;

        public StringConstant(Token value) : base(typeof(string))
        {
            this.token = value;
        }

        public string Value => token.value;

        public override string StringValue => Value;
    }
}