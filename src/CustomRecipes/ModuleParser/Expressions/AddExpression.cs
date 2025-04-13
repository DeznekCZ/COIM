namespace CustomRecipes.Python
{
    public class AddExpression : ABinaryOperatorExpression
    {
        public AddExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__add__(left, right);
        }
    }
}