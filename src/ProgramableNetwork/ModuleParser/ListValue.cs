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

        public int IntValue => throw new System.NotImplementedException();

        public long LongValue => throw new System.NotImplementedException();

        public bool BooleanValue => throw new System.NotImplementedException();

        public IEnumerator<IExpression> GetEnumerator()
        {
            return listItems.GetEnumerator();
        }

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.InvalidCastException("can not be referenced");
        }

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return listItems.Select(i => i.GetValue(context)).ToList();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return listItems.GetEnumerator();
        }
    }
}