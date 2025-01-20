namespace ProgramableNetwork.Python
{
    internal class BitOrExpression : IExpression
    {
        private IExpression expression;
        private IExpression f;

        public BitOrExpression(IExpression expression, IExpression f)
        {
            this.expression = expression;
            this.f = f;
        }
    }
}