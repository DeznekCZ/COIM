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
    }
}