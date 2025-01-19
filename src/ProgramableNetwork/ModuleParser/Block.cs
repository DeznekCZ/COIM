using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class Block
    {
        public readonly List<Import> imports = new List<Import>();
        public readonly Dictionary<string, ClassDefinition> classes = new Dictionary<string, ClassDefinition>();
        public readonly Dictionary<string, FunctionDefinition> functions = new Dictionary<string, FunctionDefinition>();
        public readonly Block parent;
        public readonly List<IStatement> statements = new List<IStatement>();

        public Block(Block parent = null)
        {
            this.parent = parent;
        }

        public void Add(ClassDefinition @class)
        {
            classes.Add(@class.Name, @class);
            statements.Add(@class);
        }

        public void Add(Import import)
        {
            imports.Add(import);
            statements.Add(import);
        }

        public void Add(FunctionDefinition function)
        {
            functions.Add(function.Name, function);
            statements.Add(function);
        }

        public void Add(IStatement import)
        {
            statements.Add(import);
        }

        internal void Add(object v)
        {
            throw new NotImplementedException();
        }
    }
}