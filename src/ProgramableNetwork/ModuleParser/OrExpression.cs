namespace ProgramableNetwork.Python
{
    internal class OrExpression : IExpression
    {
        private IExpression con;
        private IExpression expression;

        public OrExpression(IExpression con, IExpression expression)
        {
            this.con = con;
            this.expression = expression;
        }
    }
}