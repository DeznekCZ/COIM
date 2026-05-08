using Mafi;
using PythonAPI.Expressions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PythonAPI.Statements
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
            Executed(context);
        }

        private bool Executed(IDictionary<string, object> context)
        {
            if (parent != null && parent.Executed(context)) {
				return true; // already executed
			}

			if (this.condition is null)
            {
                foreach (var item in block.statements)
                {
                    item.Execute(context);
                }
                return true;
            }

            object condition = this.condition.GetValue(context);

            if (condition is null) {
				return false;
			}

			if (condition is bool b && !b) {
				return false;
			}

			if (Expressions.Expressions.__fix__(condition) <= Fix32.Zero) {
				return false;
			}

			foreach (var item in block.statements)
            {
                item.Execute(context);
            }
            return true;
        }

		public Task ExecuteAsync(IDictionary<string, object> context)
		{
			return ExecutedAsync(context);
		}

        private async Task<bool> ExecutedAsync(IDictionary<string, object> context)
        {
            if (parent != null && await parent.ExecutedAsync(context)) {
				return true; // already executed
			}

			if (this.condition is null)
            {
                foreach (var item in block.statements)
                {
                    await item.ExecuteAsync(context);
                }
                return true;
            }

            object condition = await this.condition.GetValueAsync(context);

            if (condition is null) {
				return false;
			}

			if (condition is bool b && !b) {
				return false;
			}

			if (Expressions.Expressions.__fix__(condition) <= Fix32.Zero) {
				return false;
			}

			foreach (var item in block.statements)
            {
                await item.ExecuteAsync(context);
            }
            return true;
        }
    }
}