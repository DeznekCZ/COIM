namespace ProgramableNetwork.Python
{
    internal class FullRangeFrom : IRange
    {
        private IExpression start;
        private IExpression step;

        public FullRangeFrom(IExpression start, IExpression step = null)
        {
            this.start = start;
            this.step = step ?? new NumberConstant(1);
        }
    }
}