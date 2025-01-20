namespace ProgramableNetwork.Python
{
    internal class LowerExpression : IExpression
    {
        private IExpression bitvise;
        private IExpression expression;

        public LowerExpression(IExpression bitvise, IExpression expression)
        {
            this.bitvise = bitvise;
            this.expression = expression;
        }
    }
}