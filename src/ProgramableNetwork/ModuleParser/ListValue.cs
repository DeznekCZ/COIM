using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class ListValue : IExpression, IEnumerable<IExpression>
    {
        private Token listStart;
        private Token listEnd;
        private List<IExpression> listItems;

        public ListValue(Token listStart, Token listEnd, List<IExpression> listItems)
        {
            this.listStart = listStart;
            this.listEnd = listEnd;
            this.listItems = listItems;
        }

        public string StringValue => "[" + string.Join(",", listItems.Select(i => i.StringValue)) + "]";

        public IEnumerator<IExpression> GetEnumerator()
        {
            return listItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return listItems.GetEnumerator();
        }
    }
}