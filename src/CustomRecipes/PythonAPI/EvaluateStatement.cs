using PythonAPI.Expressions;
using PythonAPI.Statements;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PythonAPI
{
    internal class EvaluateStatement : IStatement
    {
        private IExpression expression;

        /// 1-based source line where this statement's first token sits. Populated by
        /// Lexer.ParseBlock so the editor can splice the original file by line range
        /// without re-tokenising. 0 means "not populated" (statement built without
        /// source position — e.g. from synthetic code, not a parsed file).
        public int StartLine;

        /// 1-based source line where this statement's last consumed token sits.
        /// See StartLine for population semantics.
        public int EndLine;

        public IExpression Expression => expression;

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