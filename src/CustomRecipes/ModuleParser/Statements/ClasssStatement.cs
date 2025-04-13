using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Xml.Linq;

namespace CustomRecipes.Python
{
    public class ClasssStatement : IStatement
    {
        public readonly List<IExpression> baseClasses;
        public readonly Block block;
        private readonly Token className;

        public ClasssStatement(Token className, List<IExpression> baseClasses, Block block)
        {
            this.className = className;
            this.baseClasses = baseClasses;
            this.block = block;
        }

        public string Name => className.value;
        public Dictionary<string, FunctionStatement> Functions => block.functions;

        public Dictionary<string, IExpression> Variables => block.statements
            .Where(s => s is AssignmentStatement)
            .Select(s => (AssignmentStatement)s)
            .ToDictionary(s => s.Name, s => s.Value);

        public void Execute(IDictionary<string, object> context)
        {
            IDictionary<string, object> classContext = new ChildContext(context);
            foreach (var item in block.statements)
            {
                if (item is FunctionStatement f)
                    classContext[f.Name] = new Method((args) =>
                    {
                        IDictionary<string, object> methodContext = new ChildContext(classContext);
                        f.Arguments.AsEnumerable()
                            .Zip(args, (a, b) => (a, b.Value))
                            .Select(pair => methodContext[pair.a] = pair.Value)
                            .ToList();
                        f.Execute(methodContext);
                        return methodContext.TryGetValue("__return__", out object r) ? r : null;
                    }, f.Arguments.ToArray());
                else
                    item.Execute(classContext);
            }

            Type[] types = baseClasses.Select(b => (Type)(object)b.GetValue(context)).ToArray();
            Class @class = new Class(Name, types, classContext);
            context[Name] = @class;
        }
    }
}