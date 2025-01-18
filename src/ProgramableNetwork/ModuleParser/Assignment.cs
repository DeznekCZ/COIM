using System;

namespace ProgramableNetwork.Python
{
    public class Assignment : IStatement
    {
        private IExpression target;
        private IExpression value;

        public Assignment(IExpression qualifiedName, IExpression expression)
        {
            this.target = qualifiedName;
            this.value = expression;
        }

        public string Name => target is QualifiedName name ? name.Concat : throw new InvalidCastException("Target is not qualified name");

        public IExpression Value => value;
    }
}