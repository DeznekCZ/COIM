using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python
{
    internal class EvaluateStatement : IStatement
    {
        private IExpression expression;

        public EvaluateStatement(IExpression expression)
        {
            this.expression = expression;
        }

        public void Execute(IDictionary<string, object> context)
        {
            this.expression.GetValue(context);
        }

		public Task ExecuteAsync(IDictionary<string, object> context) {
			return this.expression.GetValueAsync(context);
		}
	}
}