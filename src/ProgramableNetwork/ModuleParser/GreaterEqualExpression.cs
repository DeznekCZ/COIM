namespace ProgramableNetwork.Python
{
    internal class GreaterEqualExpression : IExpression
    {
        private IExpression bitvise;
        private IExpression expression;

        public GreaterEqualExpression(IExpression bitvise, IExpression expression)
        {
            this.bitvise = bitvise;
            this.expression = expression;
        }
    }
}