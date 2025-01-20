namespace ProgramableNetwork.Python
{
    internal class NotExpression : IExpression
    {
        private IExpression expression;

        public NotExpression(IExpression expression)
        {
            this.expression = expression;
        }
    }
}