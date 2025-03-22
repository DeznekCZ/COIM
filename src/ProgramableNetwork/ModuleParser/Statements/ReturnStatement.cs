using ProgramableNetwork.Python;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Python
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
    }
}
