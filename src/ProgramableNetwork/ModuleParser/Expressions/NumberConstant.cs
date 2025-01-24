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
            this.Value = int.TryParse(value.value, out int i) ? i
                       : float.Parse(value.value);
        }

        public NumberConstant(int v) : base(typeof(Fix32))
        {
            this.token = new Token(new System.IO.FileInfo("f"), 0, 0, 1, PythonTokens.number, v.ToString());
            this.Value = v;
        }

        private readonly Token token;
        private int v;

        public object Value { get; }

        public override dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return Value;
        }
    }
}