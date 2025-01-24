
namespace ProgramableNetwork.Python
{
    internal class ModExpression : ABinaryOperatorExpression
    {
        public ModExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        protected override object Evaluate(object left, object right)
        {
            return Expressions.__mod__(left, ZeroCheck(left: false, "Can not divide by zero: {0}"));
        }
    }
}