using Mafi;
using System;

namespace ProgramableNetwork.Python
{
    public class ModuleEntityFieldProtoDefinition : IModuleFieldProtoDefinition
    {
        public readonly Type type;
        public readonly string id;
        public readonly string name;
        public readonly string desc;
        public readonly Fix32 distance;

        public ModuleEntityFieldProtoDefinition(Type type, string id, string name, string desc, Fix32 distance)
        {
            if (type is null)
                throw new NullReferenceException("Invalid entity type");
            this.type = type;
            this.id = id;
            this.name = name;
            this.desc = desc;
            this.distance = distance;
        }
    }
}