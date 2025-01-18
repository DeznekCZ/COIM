using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class Class : IStatement
    {
        public readonly List<QualifiedName> baseClasses;
        public readonly Block block;
        private readonly Token className;

        public Class(Token className, List<QualifiedName> baseClasses, Block block)
        {
            this.className = className;
            this.baseClasses = baseClasses;
            this.block = block;
        }

        public string Name => className.value;
        public Dictionary<string, Function> Functions => block.functions;

        public Dictionary<string, IExpression> Variables => block.statements
            .Where(s => s is Assignment)
            .Select(s => (Assignment)s)
            .ToDictionary(s => s.Name, s => s.Value);
    }
}