using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomRecipes.Python
{
    public class ImportStatement : IStatement
    {
        public readonly IExpression name;
        public readonly List<Token> exportedItems;

        public ImportStatement(IExpression name, List<Token> exportedItems)
        {
            this.name = name;
            this.exportedItems = exportedItems;
        }

        public void Execute(IDictionary<string, object> context)
        {
            string name = this.name.Path;

            if (name.StartsWith("Mafi"))
            {
                foreach (var item in exportedItems)
                {
                    if (item.value == "*")
                    {
                        throw new PythonParseException(item, "Cannot use * for import of captain of insustry classes");
                    }

                    string fullName = name + "." + item.value;
                    context[item.value] = AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .SelectMany(a => a.GetExportedTypes())
                        .First(t => t.FullName == fullName);
                }
            }

            else if (name.StartsWith("CustomRecipes"))
            {
                // SKIP, all stuff is initialized
                foreach (var item in exportedItems)
                {
                    string fullName = name + "." + item.value;
                    if (!context.ContainsKey(item.value)) {
                        throw new PythonParseException(item, "Missing implementation of: " + fullName);
                    }
                }
            }

            else
            {
                throw new System.NotImplementedException(name);
            }
        }
    }
}