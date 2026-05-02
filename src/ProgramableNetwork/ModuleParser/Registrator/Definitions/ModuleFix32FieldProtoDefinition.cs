using Mafi;

namespace ProgramableNetwork.Python
{
    public class ModuleFix32FieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public ModuleFix32FieldProtoDefinition(string id, string name, string desc, Fix32? defaultValue, bool overrideInput = false, bool showInTooltip = false)
        {
            this.id = id;
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
            this.overrideInput = overrideInput;
            this.showInTooltip = showInTooltip;
        }

        public readonly string id;
        public readonly string name;
        public readonly string desc;
        public readonly Fix32? defaultValue;
        public readonly bool overrideInput;
        public readonly bool showInTooltip;
    }
}