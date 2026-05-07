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
        // When true, the registrator routes this entry through SharedFieldLabels so
        // its display label is registered ONCE under ProgramableNetwork_PinOrField_<name>
        // instead of being minted per-module.
        public readonly bool shared;

        public ModuleConnectorProtoDefinition(string id, string name, bool shared = false)
        {
            this.id = id;
            this.name = name;
            this.width = 1;
            this.defaultText = "";
            this.shared = shared;
        }

        public ModuleConnectorProtoDefinition(string id, string name, int width, string defaultText)
        {
            this.id = id;
            this.name = name;
            this.width = width;
            this.defaultText = defaultText ?? "";
            this.shared = false;
        }
    }
}
