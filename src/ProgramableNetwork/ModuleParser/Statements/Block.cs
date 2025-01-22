using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public class Block
    {
        public readonly List<ImportStatement> imports = new List<ImportStatement>();
        public readonly Dictionary<string, ClasssStatement> classes = new Dictionary<string, ClasssStatement>();
        public readonly Dictionary<string, FunctionStatement> functions = new Dictionary<string, FunctionStatement>();
        public readonly Block parent;
        public readonly List<IStatement> statements = new List<IStatement>();

        public Block(Block parent = null)
        {
            this.parent = parent;
        }

        public void Add(ClasssStatement @class)
        {
            classes.Add(@class.Name, @class);
            statements.Add(@class);
        }

        public void Add(ImportStatement import)
        {
            imports.Add(import);
            statements.Add(import);
        }

        public void Add(FunctionStatement function)
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