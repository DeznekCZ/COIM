namespace ProgramableNetwork.Python
{
    public class IsExpression : IExpression
    {
        private IExpression finalExpression;
        private bool not;
        private QualifiedName qualifiedName;

        public IsExpression(IExpression finalExpression, bool not, QualifiedName qualifiedName)
        {
            this.finalExpression = finalExpression;
            this.not = not;
            this.qualifiedName = qualifiedName;
        }

        public string StringValue => throw new System.NotImplementedException();
    }
}