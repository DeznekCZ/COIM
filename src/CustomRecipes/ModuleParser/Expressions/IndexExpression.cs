namespace CustomRecipes.Python
{
    public class IndexExpression : ABinaryOperatorExpression
    {
        public IndexExpression(IExpression left, IRange right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            if (right is Range range)
                return Expressions.__range__(left, range);
            else
                return Expressions.__index__(left, right);
        }
    }
}