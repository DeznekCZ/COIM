namespace ProgramableNetwork.Python
{
    internal class InExpression : IExpression
    {
        private IExpression expression;
        private IExpression list;

        public InExpression(IExpression expression, IExpression list)
        {
            this.expression = expression;
            this.list = list;
        }
    }
}