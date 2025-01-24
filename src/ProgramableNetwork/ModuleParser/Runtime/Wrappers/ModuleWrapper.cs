using Mafi;
using Mafi.Core.Entities;

namespace ProgramableNetwork.Python
{
    public class ModuleWrapper
    {
        public readonly Module module;
        public readonly Class @class;

        public ModuleWrapper(Module module, Class @class)
        {
            this.module = module;
            this.@class = @class;
        }

        public FieldSetter field => new FieldSetter(module);

        public class FieldSetter
        {
            private Module module;

            public FieldSetter(Module module)
            {
                this.module = module;
            }

            public Entity get_ent(string fieldName)
            {
                return module.Field.Entity<Entity>(fieldName);
            }

            public bool get_bool(string name, bool value)
            {
                return module.Field[name, (value ? 1 : 0).ToFix32()] > 0;
            }

            public void set_bool(string name, bool value)
            {
                module.Field[name] = (value ? 1 : 0).ToFix32();
            }

            public Fix32 get(string name, Fix32 value)
            {
                return module.Field[name, value];
            }

            public void set(string name, Fix32 value)
            {
                module.Field[name] = value;
            }
        }

        public InputSetter input => new InputSetter(module);

        public class InputSetter
        {
            private Module module;

            public InputSetter(Module module)
            {
                this.module = module;
            }

            public bool get_bool(string name, bool value)
            {
                return module.Input[name, (value ? 1 : 0).ToFix32()] > 0;
            }

            public void set_bool(string name, bool value)
            {
                module.Input[name] = (value ? 1 : 0).ToFix32();
            }

            public Fix32 get(string name, Fix32 value)
            {
                return module.Input[name, value];
            }

            public void set(string name, Fix32 value)
            {
                module.Input[name] = value;
            }
        }

        public OutputSetter output => new OutputSetter(module);

        public class OutputSetter
        {
            private Module module;

            public OutputSetter(Module module)
            {
                this.module = module;
            }

            public bool get_bool(string name, bool value)
            {
                return module.Output[name, (value ? 1 : 0).ToFix32()] > 0;
            }

            public void set_bool(string name, bool value)
            {
                module.Output[name] = (value ? 1 : 0).ToFix32();
            }

            public Fix32 get(string name, Fix32 value)
            {
                return module.Output[name, value];
            }

            public void set(string name, Fix32 value)
            {
                module.Output[name] = value;
            }
        }
    }
}