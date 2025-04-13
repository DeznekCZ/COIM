using System;
using System.Collections.Generic;

namespace CustomRecipes.Python
{
    internal class Constructor : IExpression
    {
        private Func<NamedValue[], object> value;

        public Constructor(Func<NamedValue[], object> value, string[] arguments = null)
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

        public object Invoke(NamedValue[] args)
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