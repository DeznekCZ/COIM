using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    public class Template
    {
        public string Name { get; }
        public ModuleProto ModuleProto { get; }
        public Action<Module> Setting { get; }

        public Template(string name, ModuleProto moduleProto, Action<Module> value)
        {
            this.Name = name;
            this.ModuleProto = moduleProto;
            this.Setting = value;
        }
    }
}
