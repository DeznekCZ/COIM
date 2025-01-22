using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class ExpressionStatement : IStatement
    {
        private IExpression v;

        public ExpressionStatement(IExpression v)
        {
            this.v = v;
        }

        public void Execute(IDictionary<string, object> context)
        {
            if (v is IStatement s)
            {
                s.Execute(context);
            }
            else if (v is Call c)
            {
                c.Invoke(context);
            }
        }
    }
}