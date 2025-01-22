namespace ProgramableNetwork.Python
{
    internal class PropertyExpression : AUnaryOperatorExpression
    {
        private string value;

        public PropertyExpression(IExpression expression, string value)
            : base(expression)
        {
            this.value = value;
        }

        protected override object Evaluate(object value)
        {
            NullCheck("Can not get property of None {0}");

            return value;
        }
    }
}