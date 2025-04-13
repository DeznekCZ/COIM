using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public class VariableExpression : IExpression
    {
        private string value;

        public VariableExpression(string value)
        {
            this.value = value;
        }

        public string Path => value;

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            return new Reference<object>(
                    (value) => context[this.value] = value,
                    () => context[this.value]
                );
        }

        public object GetValue(IDictionary<string, object> context)
        {
            return context.TryGetValue(this.value, out object value) ? value : null;
        }
    }
}