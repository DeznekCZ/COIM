using System.Collections.Generic;

namespace ProgramableNetwork
{
    public struct ControllerTemplate
    {
        public delegate void Settings();
        public delegate Settings ModulePlacement(Controller controller);

        public string id;
        public string name;
        public string description;
        public ModulePlacement modules;

        public ControllerTemplate(string id, string name, string description, ModulePlacement modules)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.modules = modules;
        }

        public override bool Equals(object obj)
        {
            return obj is ControllerTemplate other &&
                   id == other.id &&
                   name == other.name &&
                   description == other.description &&
                   EqualityComparer<ModulePlacement>.Default.Equals(modules, other.modules);
        }

        public override int GetHashCode()
        {
            int hashCode = -1055989087;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(description);
            hashCode = hashCode * -1521134295 + EqualityComparer<ModulePlacement>.Default.GetHashCode(modules);
            return hashCode;
        }

        public void Deconstruct(out string id, out string name, out string description, out ModulePlacement modules)
        {
            id = this.id;
            name = this.name;
            description = this.description;
            modules = this.modules;
        }

        public static implicit operator (string id, string name, string description, ModulePlacement modules)(ControllerTemplate value)
        {
            return (value.id, value.name, value.description, value.modules);
        }

        public static implicit operator ControllerTemplate((string id, string name, string description, ModulePlacement modules) value)
        {
            return new ControllerTemplate(value.id, value.name, value.description, value.modules);
        }
    }
}