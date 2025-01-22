namespace ProgramableNetwork.Python
{
    internal class VariableExpression : IExpression
    {
        private string value;

        public VariableExpression(string value)
        {
            this.value = value;
        }
    }
}