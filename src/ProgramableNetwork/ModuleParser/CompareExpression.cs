namespace ProgramableNetwork.Python
{
    internal class CompareExpression : IExpression
    {
        private IExpression finalExpression;
        private Token comp;

        public CompareExpression(IExpression finalExpression, Token comp)
        {
            this.finalExpression = finalExpression;
            this.comp = comp;
        }

        public string StringValue => throw new System.NotImplementedException();
    }
}