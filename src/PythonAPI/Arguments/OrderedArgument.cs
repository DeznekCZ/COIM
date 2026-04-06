using PythonAPI.Expressions;

namespace PythonAPI.Arguments
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