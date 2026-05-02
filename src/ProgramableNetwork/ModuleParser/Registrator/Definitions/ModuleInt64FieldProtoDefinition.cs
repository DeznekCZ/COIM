namespace ProgramableNetwork.Python
{
    public class ModuleInt64FieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public ModuleInt64FieldProtoDefinition(string id, string name, string desc, long defaultValue, bool overrideInput = false, bool showInTooltip = false)
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
        public readonly long defaultValue;
        public readonly bool overrideInput;
        public readonly bool showInTooltip;
    }
}