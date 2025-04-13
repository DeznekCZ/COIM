namespace CustomRecipes.Python
{
    public class PowerExpression : ABinaryOperatorExpression
    {
        public PowerExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__pow__(
                NullCheck(left: true, "Powered value can not be None: {0}"),
                NullCheck(left: true, "Power value can not be None: {0}")
            );
        }
    }
}