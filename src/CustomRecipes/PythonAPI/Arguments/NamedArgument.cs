using PythonAPI;
using PythonAPI.Expressions;

namespace PythonAPI.Arguments
{
    internal class NamedArgument : IArgument
    {
        private Token name;

        public NamedArgument(Token name, IExpression expression)
        {
            this.name = name;
            this.Name = name.value;
            this.Expression = expression;
        }

        public string Name { get; }
        public IExpression Expression { get; }
    }
}