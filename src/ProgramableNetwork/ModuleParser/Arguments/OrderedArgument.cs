namespace ProgramableNetwork.Python
{
    internal class OrderedArgument : IArgument
    {
        private IExpression expression;

        public OrderedArgument(IExpression expression)
        {
            this.expression = expression;
        }

        public IExpression Expression => expression;
    }
}