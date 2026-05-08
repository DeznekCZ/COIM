using PythonAPI.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Statements
{
    public class ReturnStatement : IStatement
    {
        public IExpression Expression { get; }

        public ReturnStatement(IExpression expression = null)
        {
            this.Expression = expression;
        }

        public void Execute(IDictionary<string, object> context)
        {
            throw new ReturnException( Expression?.GetValue(context) );
        }
		public async Task ExecuteAsync(IDictionary<string, object> context) {
			throw new ReturnException( await (Expression?.GetValueAsync(context) ?? Task.FromResult<object>(null)) );
		}
	}
}
