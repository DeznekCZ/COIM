using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    public interface IStatement
    {
        void Execute(IDictionary<string, dynamic> context);
    }
}