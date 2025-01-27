namespace ProgramableNetwork.Python
{
    internal class GreaterExpression : ABinaryOperatorExpression, IComparison
    {
        public GreaterExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        public IExpression Left => left;

        public IExpression Right => right;

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__ge__(left, right);
        }
    }
}