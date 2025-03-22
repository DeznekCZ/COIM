using System;

namespace ProgramableNetwork.Python
{
    [Serializable]
    internal class PythonParseException : Exception
    {
        public PythonParseException(Token token, string message)
            : base($"[{token.line}:{token.column}]: {message}")
        {
        }
    }
}