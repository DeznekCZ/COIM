namespace ProgramableNetwork.Python
{
    internal class SumExpression : IExpression
    {
        private IExpression item2;
        private IExpression f;

        public SumExpression(IExpression item2, IExpression f)
        {
            this.item2 = item2;
            this.f = f;
        }
    }
}