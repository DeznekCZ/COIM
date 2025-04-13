using System;
using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public class CallExpression : IExpression
    {
        private IExpression expression;
        private List<IArgument> arguments;

        public string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

        public IExpression Calle => expression;
        public List<IArgument> Arguments => arguments;

        public CallExpression(IExpression expression, List<IArgument> expressions)
        {
            this.expression = expression;
            this.arguments = expressions;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            object executable = expression.GetValue(context);
            if (executable == null) {
                throw new NullReferenceException($"Null can not be called: {expression}");
            }

            List<(string name, object value)> arguments = new List<(string name, object value)>();
            foreach (IArgument argument in this.arguments)
            {
                if (argument is NamedArgument named)
                    arguments.Add((named.Name, named.Expression.GetValue(context)));
                else
                    arguments.Add(((string name, object value))(null, argument.Expression.GetValue(context)));
            }

            return Expressions.__call__(executable, arguments);
        }
    }
}