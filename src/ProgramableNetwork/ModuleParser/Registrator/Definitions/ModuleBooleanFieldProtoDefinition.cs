namespace ProgramableNetwork.Python
{
    public class ModuleBooleanFieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public ModuleBooleanFieldProtoDefinition(string id, string name, string desc, bool defaultValue, bool overrideInput = false)
        {
            this.id = id;
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
            this.overrideInput = overrideInput;
        }

        public readonly string id;
        public readonly string name;
        public readonly string desc;
        public readonly bool defaultValue;
        public readonly bool overrideInput;
    }
}