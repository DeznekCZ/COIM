namespace ProgramableNetwork.Python
{
    internal class FullRange : IRange
    {
        private IExpression step;

        public FullRange(IExpression step = null)
        {
            this.step = step ?? new NumberConstant(0);
        }
    }
}