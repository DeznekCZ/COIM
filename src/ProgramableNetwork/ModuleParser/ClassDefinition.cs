using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Xml.Linq;

namespace ProgramableNetwork.Python
{
    public class ClassDefinition : IStatement
    {
        public readonly List<QualifiedName> baseClasses;
        public readonly Block block;
        private readonly Token className;

        public ClassDefinition(Token className, List<QualifiedName> baseClasses, Block block)
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

        public void Execute(IDictionary<string, object> context)
        {
            Class @class = new Class(Name, baseClasses.Select(b => (Type)(object)b.GetValue(context)).ToArray());
            IDictionary<string, object> classContext = new ChildContext(context);
            foreach (var item in block.statements)
            {
                if (item is Function f)
                    classContext[f.Name] = new Constructor((args) =>
                    {
                        IDictionary<string, object> methodContext = new ChildContext(classContext);
                        f.Execute(context);
                        return context.TryGetValue("__return__", out object r) ? r : null;
                    });
                else
                    item.Execute(classContext);
            }
            context[Name] = classContext;
        }
    }
}