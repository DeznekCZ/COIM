using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class QualifiedName : IExpression
    {
        public readonly List<Token> names = new List<Token>();

        public string Concat => string.Join(".", names.Select(n => n.value));

        public string StringValue => throw new System.NotImplementedException("could not evaluate");
    }
}