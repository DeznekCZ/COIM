namespace ProgramableNetwork.Python
{
    internal class MulExpression : ABinaryOperatorExpression
    {
        public MulExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__mul__(left, right);
        }
    }
}