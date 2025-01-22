namespace ProgramableNetwork.Python
{
    internal class ShiftLeftExpression : IExpression
    {
        private IExpression item2;
        private IExpression f;

        public ShiftLeftExpression(IExpression item2, IExpression f)
        {
            this.item2 = item2;
            this.f = f;
        }
    }
}