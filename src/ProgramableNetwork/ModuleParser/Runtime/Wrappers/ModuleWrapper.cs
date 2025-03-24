using Mafi;
using Mafi.Collections;
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

        public FieldSetter Field => new FieldSetter(module);

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

        public InputSetter Input => new InputSetter(module);

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

        public OutputSetter Output => new OutputSetter(module);

        public class OutputSetter
        {
            private Module module;

            public OutputSetter(Module module)
            {
                this.module = module;
            }

            public bool get_bool(string name, bool value)
            {
                return module.Output.Bool[name];
            }

            public void set_bool(string name, bool value)
            {
                module.Output.Bool[name] = value;
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

        public ModuleStatus Status
        {
            get => module.Status;
            set => module.SetStatus(value);
        }

        public string Error
        {
            get => module.Error;
            set => module.SetError(value);
        }

        public bool Info
        {
            get => module.Info;
            set => module.Info = value;
        }

        public bool Warning
        {
            get => module.Warning;
            set => module.Warning = value;
        }

        public Dict<string, int> NumberData => module.NumberData;
        public Dict<string, string> StringData => module.StringData;
        public ModuleProto Prototype => module.Prototype;
        public Controller Controller => module.Controller;
    }
}