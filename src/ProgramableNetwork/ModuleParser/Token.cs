
using System.IO;

namespace ProgramableNetwork.Python
{
    public class Token
    {
        public static Token EOF = new Token(new FileInfo("end-of-file"), "", 0, 0, 0, PythonTokens.eof, "");
        public readonly FileInfo source;
        public readonly string lineText;
        public readonly int line;
        public readonly int column;
        public readonly int length;
        public readonly PythonTokens type;
        public readonly string value;

        public Token(FileInfo source, string lineText, int line, int column, int length, PythonTokens type, string value)
        {
            this.source = source;
            this.lineText = lineText;
            this.line = line;
            this.column = column;
            this.length = length;
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            return $"(\"{value}\" [{type}] at {source}({line}:{column}): )";
        }
    }
}