using ProgramableNetwork.Python;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Python
{
    internal class ReturnStatement : IStatement
    {
        private IExpression expression;

        public ReturnStatement(IExpression expression = null)
        {
            this.expression = expression;
        }

        public void Execute(IDictionary<string, object> context)
        {
            throw new ReturnException( expression?.GetValue(context) );
        }
    }
}
