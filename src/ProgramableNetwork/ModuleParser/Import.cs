using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class Import : IStatement
    {
        public readonly QualifiedName name;
        public readonly List<Token> exportedItems;

        public Import(QualifiedName name, List<Token> exportedItems)
        {
            this.name = name;
            this.exportedItems = exportedItems;
        }
    }
}