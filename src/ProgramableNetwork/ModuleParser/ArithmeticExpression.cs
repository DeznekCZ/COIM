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
    }
}