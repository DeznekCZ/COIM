namespace ProgramableNetwork.Python
{
    internal class LowerExpression : ABinaryOperatorExpression
    {
        public LowerExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__lt__(left, right);
        }
    }
}