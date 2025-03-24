using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public partial class Module
    {
        public class FieldOrInputData
        {
            private Module module;

            public ReadNumberReference<bool> Bool { get; }
            public ReadNumberReference<int> Integer { get; }

            internal FieldOrInputData(Module module)
            {
                this.module = module;
                this.Bool = new ReadNumberReference<bool>(
                    this, // reference
                    f => f > Fix32.Zero,
                    b => b ? Fix32.One : Fix32.Zero,
                    false
                );
                this.Integer = new ReadNumberReference<int>(
                    this, // reference
                    f => f.IntegerPart,
                    i => Fix32.FromInt(i),
                    0
                );
            }

            public Fix32 this[string name, Fix32 defaultValue]
            {
                get => module.Field.Bool["field_" + name]
                    ? module.Field[name, defaultValue]
                    : module.Input[name, defaultValue];
            }

            public Fix32 this[string name]
            {
                get => this[name, Fix32.Zero];
            }

            public T Entity<T>(string name)
                where T : class, IEntity
            {
                return module.Field.Bool["field_" + name]
                    ? module.Field.Entity<T>(name)
                    : module.Input.Entity<T>(name);
            }

            public ProductProto Product(string name)
            {
                return module.Field.Bool["field_" + name]
                    ? module.Field.Product(name)
                    : module.Input.Product(name);
            }

            public IProtoWithIcon EntityProtoIconified(string name)
            {
                return module.Field.Bool["field_" + name]
                    ? module.Field.EntityProtoIconified(name)
                    : module.Input.EntityProtoIconified(name);
            }
        }
    }
}