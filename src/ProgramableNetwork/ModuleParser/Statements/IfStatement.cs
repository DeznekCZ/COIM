using Mafi;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    internal class IfStatement : IStatement
    {
        private IExpression condition;
        private IfStatement parent;
        private Block block;

        public IfStatement(IExpression condition, Block block)
        {
            this.condition = condition;
            this.block = block;
        }

        public IfStatement(IfStatement parent, Block block)
        {
            this.parent = parent;
            this.block = block;
        }

        public IfStatement(IfStatement parent, IExpression condition, Block block)
        {
            this.parent = parent;
            this.condition = condition;
            this.block = block;
        }

        public void Execute(IDictionary<string, object> context)
        {
            if (parent == null || !parent.Executed(context))
            {
                Executed(context);
            }
        }

        private bool Executed(IDictionary<string, object> context)
        {
            object condition = this.condition == null ? true : this.condition.GetValue(context);
            if ((condition is null) || (condition is bool b && !b) || (Expressions.__fix__(condition) == Fix32.Zero))
            {
                return false;
            }
            foreach (var item in block.statements)
            {
                item.Execute(context);
            }
            return true;
        }
    }
}