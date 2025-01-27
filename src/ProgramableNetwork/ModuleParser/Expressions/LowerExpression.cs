namespace ProgramableNetwork.Python
{
    internal class LowerExpression : ABinaryOperatorExpression, IComparison
    {
        public LowerExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        public IExpression Left => left;

        public IExpression Right => right;

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__lt__(left, right);
        }
    }
}