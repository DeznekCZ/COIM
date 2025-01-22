namespace ProgramableNetwork.Python
{
    internal class ModExpression : IExpression
    {
        private IExpression item2;
        private IExpression f;

        public ModExpression(IExpression item2, IExpression f)
        {
            this.item2 = item2;
            this.f = f;
        }
    }
}