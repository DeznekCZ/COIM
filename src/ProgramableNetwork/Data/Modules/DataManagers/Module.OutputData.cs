using Mafi;
using Mafi.Core.Products;

namespace ProgramableNetwork
{
    public partial class Module
    {
        public class OutputData : IReference<Fix32>
        {
            private Module module;

            public IReference<bool> Bool { get; }
            public IReference<int> Integer { get; }

            internal OutputData(Module module)
            {
                this.module = module;
                this.Bool = new NumberReference<bool>(
                    this, // reference
                    b => b ? Fix32.One : Fix32.Zero,
                    f => f > Fix32.Zero,
                    false
                );
                this.Integer = new NumberReference<int>(
                    this, // reference
                    i => Fix32.FromInt(i),
                    f => f.IntegerPart,
                    0
                );
            }

            public Fix32 this[string name, Fix32 defaultValue]
            {
                get => module.NumberData.TryGetValue("out__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue;
            }

            public Fix32 this[string name]
            {
                get => this[name, Fix32.Zero];
                set => module.NumberData["out__" + name] = value.RawValue;
            }

            public ProductProto Product(string name)
            {
                module.NumberData.TryGetValue("out__" + name, out int slimId);

                if (slimId == 0)
                {
                    return null;
                }

                module.StringData.TryGetValue("out__" + name, out string cache);
                if (!string.IsNullOrEmpty(cache))
                { // try get entity by name and check slimId
                    Option<ProductProto> product = module.Context.ProtosDb.Get<ProductProto>(new Mafi.Core.Prototypes.Proto.ID(cache));
                    if (product.HasValue && product.Value.SlimId.Value == slimId)
                    {
                        return product.Value;
                    }
                }
                // else
                { // try get entity by slimId
                    Option<ProductProto> product = module.Context.ProtosDb.First<ProductProto>(p => p.SlimId.Value == slimId);
                    if (product.HasValue && product.Value.SlimId.Value == slimId)
                    {
                        module.StringData["out__" + name] = product.Value.Id.Value;
                        return product.Value;
                    }
                }

                module.NumberData.TryRemove("out__" + name, out slimId);
                module.StringData.TryRemove("out__" + name, out cache);
                return null;
            }
        }
    }
}