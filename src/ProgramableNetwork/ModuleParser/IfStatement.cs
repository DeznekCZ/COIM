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

        public void Execute(IDictionary<string, dynamic> context)
        {
            if (parent != null && !parent.Executed(context) && !Executed(context))
            {
                // skipping all blocks
            }
        }

        private bool Executed(IDictionary<string, dynamic> context)
        {
            dynamic condition = this.condition?.GetValue(context) ?? null;
            if ((condition is object o && o == null) || (condition is bool b && b == false) || (condition is Fix32 f && f == Fix32.Zero))
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