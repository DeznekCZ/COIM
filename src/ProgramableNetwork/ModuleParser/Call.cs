using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class Call : IExpression
    {
        private IExpression expression;
        private List<IExpression> arguments;

        public Call(IExpression expression, List<IExpression> arguments)
        {
            this.expression = expression;
            this.arguments = arguments;
        }

        public string Name => expression is QualifiedName qualifiedName ? qualifiedName.Concat : throw new InvalidCastException("Can not convert to qualified name");

        public List<IExpression> Arguments => arguments;

        public string StringValue => throw new NotImplementedException();
    }
}