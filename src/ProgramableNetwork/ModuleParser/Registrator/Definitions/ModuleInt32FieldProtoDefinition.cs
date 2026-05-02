namespace ProgramableNetwork.Python
{
    public class ModuleInt32FieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public ModuleInt32FieldProtoDefinition(string id, string name, string desc, int defaultValue, bool overrideInput = false, bool showInTooltip = false)
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
        public readonly int defaultValue;
        public readonly bool overrideInput;
        public readonly bool showInTooltip;
    }
}