using System;
using System.Collections.Generic;
using System.Linq;

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

        public int IntValue => throw new NotImplementedException();

        public long LongValue => throw new NotImplementedException();

        public bool BooleanValue => throw new NotImplementedException();

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.InvalidCastException("can not be referenced");
        }

        public object GetValue(IDictionary<string, object> context)
        {
            object callable = expression.GetValue(context);
            object[] args = Arguments.Select(e => e.GetValue(context)).ToArray();

            if (callable is Constructor c)
            {
                return c.Invoke(args);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Invoke(IDictionary<string, object> context)
        {
            GetValue(context);
        }
    }
}