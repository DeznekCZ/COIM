namespace ProgramableNetwork.Python
{
    public class ModuleStringFieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public ModuleStringFieldProtoDefinition(string id, string name, string desc, string defaultValue, bool overrideInput = false, bool multilined = false, bool showInTooltip = false)
        {
            this.id = id;
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
            this.overrideInput = overrideInput;
            this.multilined = multilined;
            this.showInTooltip = showInTooltip;
        }

        public readonly string id;
        public readonly string name;
        public readonly string desc;
        public readonly string defaultValue;
        public readonly bool overrideInput;
        public readonly bool multilined;
        public readonly bool showInTooltip;
    }
}