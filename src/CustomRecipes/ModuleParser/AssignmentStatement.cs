using System.Collections.Generic;

namespace CustomRecipes.Python
{
    internal class AssignmentStatement : IStatement
    {
        private IExpression target;
        private IExpression value;

        public AssignmentStatement(IExpression target, IExpression value)
        {
            this.target = target;
            this.value = value;
        }

        public void Execute(IDictionary<string, object> context)
        {
            Reference<object> target = this.target.GetReference(context);
            object value = this.value.GetValue(context);
            target.Value = value;
        }

        public string Name => target.Path;
        public IExpression Value => value;
    }
}