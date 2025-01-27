namespace ProgramableNetwork.Python
{
    internal class ShiftLeftExpression : ABinaryOperatorExpression
    {
        public ShiftLeftExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__lshift__(left, right);
        }
    }
}