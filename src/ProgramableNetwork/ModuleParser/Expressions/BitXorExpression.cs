using Mafi.Core.Factory.Lifts;
using System.Collections.Generic;
using UnityEngine;

namespace ProgramableNetwork.Python
{
    internal class BitXorExpression : IExpression
    {
        private IExpression left;
        private IExpression right;

        public BitXorExpression(IExpression left, IExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            return Expressions.__xor__(left.GetValue(context), right.GetValue(context));
        }
    }
}