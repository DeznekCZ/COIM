using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class SingleItem : IRange
    {
        private IExpression expression;
        private IExpression index;

        public SingleItem(IExpression expression, IExpression index)
        {
            this.expression = expression;
            this.index = index;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            object target = expression.GetValue(context);
            object index = this.index.GetValue(context);

            return new Reference<object>(
                (value) => Expressions.__setitem__(target, index, value),
                () => Expressions.__getitem__(target, index)
            );
        }

        public object GetValue(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }
    }
}