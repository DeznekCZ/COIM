using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Python
{
    public class ModuleConnectorProtoDefinition
    {
        public readonly string id;
        public readonly string name;
        public readonly int width;
        public readonly string defaultText;

        public ModuleConnectorProtoDefinition(string id, string name)
        {
            this.id = id;
            this.name = name;
            this.width = 1;
            this.defaultText = "";
        }

        public ModuleConnectorProtoDefinition(string id, string name, int width, string defaultText)
        {
            this.id = id;
            this.name = name;
            this.width = width;
            this.defaultText = defaultText ?? "";
        }
    }
}
