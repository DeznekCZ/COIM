using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class SingleItem : IRange
    {
        private IExpression index;

        public SingleItem(IExpression index)
        {
            this.index = index;
        }

        public string Path => $"[{ index.Path }]";

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            return index.GetValue(context);
        }
    }
}