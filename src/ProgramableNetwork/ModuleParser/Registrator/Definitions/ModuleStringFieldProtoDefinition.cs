namespace ProgramableNetwork.Python
{
    public class ModuleStringFieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public ModuleStringFieldProtoDefinition(string id, string name, string desc, string defaultValue)
        {
            this.id = id;
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
        }

        public readonly string id;
        public readonly string name;
        public readonly string desc;
        public readonly string defaultValue;
    }
}