using System;
using System.IO;

namespace PythonAPI
{
    /// Wraps a runtime error (e.g. NotImplementedException, ArgumentException)
    /// thrown by IStatement.Execute / IExpression.GetValue with the source file
    /// and line of the executing statement, so the .py author sees where the
    /// failure occurred instead of a bare CLR stack trace.
    [Serializable]
    public class PythonRuntimeException : Exception
    {
        public readonly FileInfo source;
        public readonly int startLine;
        public readonly int endLine;

        public PythonRuntimeException(FileInfo source, int startLine, int endLine, string message, Exception inner)
            : base(formatMessage(source, startLine, endLine, message), inner)
        {
            this.source = source;
            this.startLine = startLine;
            this.endLine = endLine;
        }

        private static string formatMessage(FileInfo source, int startLine, int endLine, string message)
        {
            string path = source?.FullName ?? "<unknown>";
            string range = startLine <= 0
                ? string.Empty
                : (endLine > startLine ? $"({startLine}-{endLine})" : $"({startLine})");
            return $"{path}{range}: {message}";
        }
    }
}
