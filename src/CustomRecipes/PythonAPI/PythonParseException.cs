using System;

namespace PythonAPI
{
    [Serializable]
    internal class PythonParseException : Exception
    {
        public readonly Token token;

        public PythonParseException(Token token, string message)
            : base($"{token.source?.FullName ?? "<unknown>"}({token.line}:{token.column}): {message}")
        {
            this.token = token;
        }
    }
}