namespace ProgramableNetwork.Python
{
    internal class GreaterExpression : IExpression
    {
        private IExpression bitvise;
        private IExpression expression;

        public GreaterExpression(IExpression bitvise, IExpression expression)
        {
            this.bitvise = bitvise;
            this.expression = expression;
        }
    }
}