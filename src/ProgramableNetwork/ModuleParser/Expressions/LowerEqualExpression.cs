namespace ProgramableNetwork.Python
{
    internal class LowerEqualExpression : ABinaryOperatorExpression, IComparison
    {
        public LowerEqualExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        public IExpression Left => left;

        public IExpression Right => right;

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__le__(left, right);
        }
    }
}