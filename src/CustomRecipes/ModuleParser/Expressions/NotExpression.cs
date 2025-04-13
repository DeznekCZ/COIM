namespace CustomRecipes.Python
{
    public class NotExpression : AUnaryOperatorExpression
    {
        public NotExpression(IExpression expression) : base(expression)
        {
        }

        protected override object Evaluate(object value)
        {
            return Expressions.__not__(NullCheck("Can not negate None: {0}"));
        }
    }
}