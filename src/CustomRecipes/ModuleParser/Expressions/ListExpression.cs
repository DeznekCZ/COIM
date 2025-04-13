using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomRecipes.Python
{
    public class ListExpression : IExpression
    {
        private List<IExpression> listItems;
        public string Path => throw new NotImplementedException($"Cannot get path from operator {GetType()}");

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