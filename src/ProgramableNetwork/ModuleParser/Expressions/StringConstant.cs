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
            v = v.Replace($"\\n", "\n");
            v = v.Replace($"\\t", "\t");
            v = v.Replace($"\\a", "\a");
            v = v.Replace($"\\r", "\r");
            this.Value = v;
        }

        public string Value { get; }

        public override object GetValue(IDictionary<string, object> context)
        {
            return Value;
        }
    }
}