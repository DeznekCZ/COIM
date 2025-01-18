using System.Collections.Generic;

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
    }
}