using System;

namespace ProgramableNetwork.Python
{
    internal class Class
    {
        private string name;
        private Type[] baseTypes;

        public Class(string name, Type[] baseTypes)
        {
            this.name = name;
            this.baseTypes = baseTypes;
        }
    }
}