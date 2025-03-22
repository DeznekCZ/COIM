using Mafi;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProgramableNetwork.Python
{
    public class NumberConstant : ConstantExpression
    {
        public NumberConstant(Token value) : base(typeof(Fix32))
        {
            this.token = value;
            this.Value = int.TryParse(value.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? (object)i
                       : long.TryParse(value.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l) ? (object)l
                       : float.TryParse(value.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? (object)f
                       : double.TryParse(value.value, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? (object)d
                       : throw new InvalidCastException("Invalid number format: " + value.value);
        }

        public NumberConstant(int v) : base(typeof(Fix32))
        {
            this.token = new Token(new System.IO.FileInfo("f"), "<builtin>", 0, 0, 1, PythonTokens.number, v.ToString());
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