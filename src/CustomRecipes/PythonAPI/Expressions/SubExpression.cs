namespace PythonAPI.Expressions
{
    public class SubExpression : ABinaryOperatorExpression
    {
        public SubExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__sub__(left, right);
        }
    }
}
