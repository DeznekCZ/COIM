namespace ProgramableNetwork.Python
{
    internal class BitXorExpression : IExpression
    {
        private IExpression expression;
        private IExpression f;

        public BitXorExpression(IExpression expression, IExpression f)
        {
            this.expression = expression;
            this.f = f;
        }
    }
}