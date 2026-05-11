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
        // True when the template should also appear in the regular module
        // picker (alongside the unconfigured ModuleProto), not just in the
        // dedicated template picker.  Use it for "mode variants" of a
        // single ModuleProto so the player picks a configured shape
        // directly, instead of dropping the module then editing fields.
        // Set via the `picker = True` class attribute in Python templates.
        public bool ShowInModulePicker { get; }

        public Template(string name, ModuleProto moduleProto, Action<Module> value, bool showInModulePicker = false)
        {
            this.Name = name;
            this.ModuleProto = moduleProto;
            this.Setting = value;
            this.ShowInModulePicker = showInModulePicker;
        }
    }
}
