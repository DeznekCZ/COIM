using PythonAPI.Expressions;
using PythonAPI.Range;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PythonAPI.Runtime;

namespace PythonAPI
{
    public class ListValue : SyncExpression, IExpression, IEnumerable<IExpression>
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

        public override string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

        public IEnumerator<IExpression> GetEnumerator()
        {
            return listItems.GetEnumerator();
        }

        public override Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            throw new System.InvalidCastException("can not be referenced");
        }

        public override dynamic GetValue(IDictionary<string, dynamic> context)
        {
            return listItems.Select(i => i.GetValue(context)).ToList();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return listItems.GetEnumerator();
        }
    }
}