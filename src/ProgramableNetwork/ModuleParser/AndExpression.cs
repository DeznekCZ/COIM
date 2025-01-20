namespace ProgramableNetwork.Python
{
    internal class AndExpression : IExpression
    {
        private IExpression inv;
        private IExpression expression;

        public AndExpression(IExpression inv, IExpression expression)
        {
            this.inv = inv;
            this.expression = expression;
        }
    }
}