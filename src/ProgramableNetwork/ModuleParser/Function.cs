using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class Function : IStatement
    {
        private Token functionName;
        private List<Token> arguments;
        private Block block;

        public Function(Token functionName, List<Token> arguments, Block block)
        {
            this.functionName = functionName;
            this.arguments = arguments;
            this.block = block;
        }

        public string Name => functionName.value;

        public IEnumerable<IStatement> Statements => block.statements;

        public List<string> Arguments => arguments.Select(arguments => arguments.value).ToList();

        public void Execute(IDictionary<string, dynamic> context)
        {
            foreach (IStatement statement in Statements)
            {
                statement.Execute(context);
            }
        }
    }
}