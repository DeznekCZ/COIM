using System;

namespace PythonAPI
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