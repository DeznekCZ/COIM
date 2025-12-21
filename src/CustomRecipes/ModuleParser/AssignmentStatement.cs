using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomAssets.Python {
	internal class AssignmentStatement : IStatement {
		private IExpression target;
		private IExpression value;

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
