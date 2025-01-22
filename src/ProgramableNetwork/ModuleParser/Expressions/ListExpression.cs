using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    internal class ListExpression : IExpression
    {
        private List<IExpression> listItems;

        public ListExpression(List<IExpression> listItems)
        {
            this.listItems = listItems;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            return listItems.Select(v => v.GetValue(context)).ToList();
        }
    }
}