using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class Assignment : IStatement, IExpression
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

        public string StringValue => throw new NotImplementedException();

        public int IntValue => throw new NotImplementedException();

        public long LongValue => throw new NotImplementedException();

        public bool BooleanValue => throw new NotImplementedException();

        public void Execute(IDictionary<string, dynamic> context)
        {
            target.GetReference(context).Value = value.GetValue(context);
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }
    }
}