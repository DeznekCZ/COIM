namespace ProgramableNetwork.Python
{
    internal class GreaterExpression : ABinaryOperatorExpression
    {
        public GreaterExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__ge__(left, right);
        }
    }
}