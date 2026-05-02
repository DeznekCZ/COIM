using System;

namespace ProgramableNetwork.Python
{
    // Runtime counterpart to PythonParseException — thrown by statements
    // and built-in helpers (for/while iteration caps, range / len argument
    // checks, dict-key misses) so PLC scripts surface known failure modes
    // with a Python-flavored message instead of a raw .NET exception type.
    // Caught by the PlcPy module action and stored in __run_error.
    [Serializable]
    public class PythonRuntimeException : Exception
    {
        public PythonRuntimeException(string message) : base(message) { }

        public PythonRuntimeException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
