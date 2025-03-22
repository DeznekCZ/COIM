namespace ProgramableNetwork.Python
{
    public class PositiveExpression : AUnaryOperatorExpression
    {
        public PositiveExpression(IExpression expression) : base(expression)
        {
        }

        protected override object Evaluate(object value)
        {
            return Expressions.__pos__(NullCheck("Cannot positive None: {0}"));
        }
    }
}