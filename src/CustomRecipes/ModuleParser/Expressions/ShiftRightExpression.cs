namespace CustomRecipes.Python
{
    public class ShiftRightExpression : ABinaryOperatorExpression
    {
        public ShiftRightExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__rshift__(left, right);
        }
    }
}