namespace ProgramableNetwork.Python
{
    internal class LowerEqualExpression : IExpression
    {
        private IExpression bitvise;
        private IExpression expression;

        public LowerEqualExpression(IExpression bitvise, IExpression expression)
        {
            this.bitvise = bitvise;
            this.expression = expression;
        }
    }
}