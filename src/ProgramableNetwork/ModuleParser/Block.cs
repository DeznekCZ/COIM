using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class Block
    {
        public readonly List<Import> imports = new List<Import>();
        public readonly Dictionary<string, Class> classes = new Dictionary<string, Class>();
        public readonly Dictionary<string, Function> functions = new Dictionary<string, Function>();
        public readonly Block parent;
        public readonly List<IStatement> statements = new List<IStatement>();

        public Block(Block parent = null)
        {
            this.parent = parent;
        }

        public void Add(Class @class)
        {
            classes.Add(@class.Name, @class);
            statements.Add(@class);
        }

        public void Add(Import import)
        {
            imports.Add(import);
            statements.Add(import);
        }

        public void Add(Function function)
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