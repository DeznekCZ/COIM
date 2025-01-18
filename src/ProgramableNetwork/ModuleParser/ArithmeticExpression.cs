using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class ArithmeticExpression : IExpression
    {
        private IExpression finalExpression;
        private Token token;

        public ArithmeticExpression(IExpression finalExpression, Token token)
        {
            this.finalExpression = finalExpression;
            this.token = token;
        }
        public string StringValue => throw new System.NotImplementedException();

        public int IntValue => throw new System.NotImplementedException();

        public long LongValue => throw new System.NotImplementedException();

        public bool BooleanValue => throw new System.NotImplementedException();

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.InvalidCastException("can not be referenced");
        }

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            throw new System.NotImplementedException();
        }
    }
}