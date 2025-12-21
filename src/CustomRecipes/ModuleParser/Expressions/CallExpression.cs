using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python
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
		public Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			throw new NotImplementedException();
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
                if (argument is NamedArgument named) {
					arguments.Add((named.Name, named.Expression.GetValue(context)));
				} else {
					arguments.Add(((string name, object value))(null, argument.Expression.GetValue(context)));
				}
			}

            return Expressions.__call__(executable, arguments);
        }
		public async Task<object> GetValueAsync(IDictionary<string, object> context) {
			object executable = await expression.GetValueAsync(context);
			if (executable == null) {
				throw new NullReferenceException($"Null can not be called: {expression}");
			}

			List<(string name, object value)> arguments = new List<(string name, object value)>();
			foreach (IArgument argument in this.arguments)
			{
				if (argument is NamedArgument named) {
					arguments.Add((named.Name, await named.Expression.GetValueAsync(context)));
				} else {
					arguments.Add(((string name, object value))(null, await argument.Expression.GetValueAsync(context)));
				}
			}

			return Expressions.__call__async__(executable, arguments);
		}
	}
}