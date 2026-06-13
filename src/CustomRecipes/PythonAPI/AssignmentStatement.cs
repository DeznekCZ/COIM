using PythonAPI.Expressions;
using PythonAPI.Statements;
using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI {
	public class AssignmentStatement : IStatement {
		private IExpression target;
		private IExpression value;

		// 1-based source line range. Populated by the lexer so the editor can
		// treat name-binding statements like `x = build_recipe(...)` the same
		// way as bare `build_recipe(...)` calls — splice the right-hand-side
		// call back at the same line range on save, walk the captured Def
		// out of model.Recipes by looking up the assigned call's StartLine.
		public int StartLine;
		public int EndLine;

		public AssignmentStatement(IExpression target, IExpression value) {
			this.target = target;
			this.value = value;
		}

		public void Execute(IDictionary<string, object> context) {
			Reference<object> target = this.target.GetReference(context);
			object value = this.value.GetValue(context);
			target.Value = value;
		}
		public async Task ExecuteAsync(IDictionary<string, object> context) {
			Reference<object> target = await this.target.GetReferenceAsync(context);
			object value = this.value.GetValueAsync(context);
			target.Value = value;
		}

		public string Name => target.Path;
		public IExpression Value => value;
	}
}
