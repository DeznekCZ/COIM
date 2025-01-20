namespace ProgramableNetwork.Python
{
    internal class EqualExpression : IExpression
    {
        private IExpression bitvise;
        private IExpression expression;

        public EqualExpression(IExpression bitvise, IExpression expression)
        {
            this.bitvise = bitvise;
            this.expression = expression;
        }
    }
}