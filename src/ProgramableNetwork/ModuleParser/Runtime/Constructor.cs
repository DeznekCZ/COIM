using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class Constructor : IExpression
    {
        private Func<IArgumentValue[], object> value;

        public Constructor(Func<IArgumentValue[], object> value, string[] arguments = null)
        {
            this.value = value;
            Arguments = arguments ?? new string[0];
        }

        public string[] Arguments { get; private set; }

        public string Path => throw new NotImplementedException();

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new InvalidCastException("Cannot be referenced");
        }

        public object GetValue(IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public object Invoke(IArgumentValue[] args)
        {
            try
            {
                return value.Invoke(args);
            }
            catch (ReturnException r)
            {
                return r.Value;
            }
        }
    }
}