using Mafi.Core.Entities;

namespace ProgramableNetwork.Python
{
    public class ModuleWrapper
    {
        private Module module;

        public ModuleWrapper(Module module)
        {
            this.module = module;
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
        }

        public InputSetter input => new InputSetter(module);

        public class InputSetter
        {
            private Module module;

            public InputSetter(Module module)
            {
                this.module = module;
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
        }
    }
}