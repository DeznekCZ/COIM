namespace ProgramableNetwork.Python
{
    public class ModuleInt64FieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public ModuleInt64FieldProtoDefinition(string id, string name, string desc, long defaultValue)
        {
            this.id = id;
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
        }

        public readonly string id;
        public readonly string name;
        public readonly string desc;
        public readonly long defaultValue;
    }
}