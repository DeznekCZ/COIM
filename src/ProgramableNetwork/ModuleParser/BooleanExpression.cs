namespace ProgramableNetwork.Python
{
    internal class BooleanExpression : IExpression
    {
        private IExpression finalExpression;
        private Token boolean;

        public BooleanExpression(IExpression finalExpression, Token boolean)
        {
            this.finalExpression = finalExpression;
            this.boolean = boolean;
        }

        public string StringValue => boolean.value.ToLower();
    }
}