using Mafi;
using System;

namespace ProgramableNetwork.Python
{
    internal class ModuleFieldProtoDefinition
    {
        private Type type;
        private string id;
        private string name;
        private string desc;
        private Fix32 distance;

        public ModuleFieldProtoDefinition(Type type, string id, string name, string desc, Fix32 distance)
        {
            this.type = type;
            this.id = id;
            this.name = name;
            this.desc = desc;
            this.distance = distance;
        }
    }
}