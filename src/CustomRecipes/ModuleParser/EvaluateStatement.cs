using System.Collections.Generic;

namespace CustomRecipes.Python
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
    }
}