using Mafi;
using PythonAPI.Expressions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PythonAPI.Statements
{
    public class IfStatement : IStatement
    {
        // Condition expression on this clause. Null for `else` branches (which
        // have a non-null Parent instead). Public read-only so editor code can
        // inspect the conditional chain when walking the AST for build_recipe
        // calls nested inside if/elif/else blocks.
        public IExpression Condition { get; }

        // The previous IfStatement in an if/elif/else chain. For a leading
        // `if`, Parent is null; subsequent `elif` and `else` clauses point at
        // the previous clause so runtime evaluation can short-circuit when a
        // higher clause already ran (see Executed()).
        public IfStatement Parent { get; }

        // Statements that run when this clause's condition matches. PackLoader
        // recurses into Block.statements looking for build_recipe calls so
        // recipes nested inside an if/elif/else are surfaced to the editor.
        public Block Block { get; }

        // Source-line bookkeeping. StartLine = the `if`/`elif`/`else` header
        // line itself (1-based). EndLine = the last line of the indented body
        // (or the same line as StartLine when the body is on the same line as
        // the header — a Python single-line if). Both are 0 when the
        // statement was constructed without a source position. Populated by
        // Lexer.Blocks.ParseIf / ParseElIf / ParseElse.
        public int StartLine;
        public int EndLine;

        public IfStatement(IExpression condition, Block block)
        {
            this.Condition = condition;
            this.Block = block;
        }

        public IfStatement(IfStatement parent, Block block)
        {
            this.Parent = parent;
            this.Block = block;
        }

        public IfStatement(IfStatement parent, IExpression condition, Block block)
        {
            this.Parent = parent;
            this.Condition = condition;
            this.Block = block;
        }

        public void Execute(IDictionary<string, object> context)
        {
            Executed(context);
        }

        private bool Executed(IDictionary<string, object> context)
        {
            if (Parent != null && Parent.Executed(context)) {
				return true; // already executed
			}

			if (this.Condition is null)
            {
                foreach (var item in Block.statements)
                {
                    item.Execute(context);
                }
                return true;
            }

            object condition = this.Condition.GetValue(context);

            if (condition is null) {
				return false;
			}

			if (condition is bool b && !b) {
				return false;
			}

			if (Expressions.Expressions.__fix__(condition) <= Fix32.Zero) {
				return false;
			}

			foreach (var item in Block.statements)
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
            if (Parent != null && await Parent.ExecutedAsync(context)) {
				return true; // already executed
			}

			if (this.Condition is null)
            {
                foreach (var item in Block.statements)
                {
                    await item.ExecuteAsync(context);
                }
                return true;
            }

            object condition = await this.Condition.GetValueAsync(context);

            if (condition is null) {
				return false;
			}

			if (condition is bool b && !b) {
				return false;
			}

			if (Expressions.Expressions.__fix__(condition) <= Fix32.Zero) {
				return false;
			}

			foreach (var item in Block.statements)
            {
                await item.ExecuteAsync(context);
            }
            return true;
        }
    }
}