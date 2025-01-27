namespace ProgramableNetwork.Python
{
    internal class LowerEqualExpression : ABinaryOperatorExpression
    {
        public LowerEqualExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__le__(left, right);
        }
    }
}