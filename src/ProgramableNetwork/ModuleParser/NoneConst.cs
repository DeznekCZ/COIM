using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class NoneConst : IExpression
    {
        private Token none;

        public NoneConst(Token none)
        {
            this.none = none;
        }

        public string StringValue => null;

        public int IntValue => 0;

        public long LongValue => 0;

        public bool BooleanValue => false;

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.NullReferenceException("null can not be referenced");
        }

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return null;
        }
    }
}