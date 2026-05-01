namespace ProgramableNetwork.Python
{
    public class BitAndExpression : ABinaryOperatorExpression
    {
        public BitAndExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__and__(left, right);
        }
    }
}
