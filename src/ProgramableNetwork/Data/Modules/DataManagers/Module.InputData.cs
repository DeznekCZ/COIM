using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public partial class Module
    {
        public class InputData : IReference<Fix32>
        {
            private Module module;

            public IReference<bool> Bool { get; }
            public IReference<int> Integer { get; }

            internal InputData(Module module)
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
                get => module.NumberData.TryGetValue("in__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue;
            }

            public Fix32 this[string name]
            {
                get => this[name, Fix32.Zero];
                set => module.NumberData["in__" + name] = value.RawValue;
            }

            public ProductProto Product(string name)
            {
                module.NumberData.TryGetValue("in__" + name, out int slimId);

                if (slimId == 0)
                {
                    return null;
                }

                module.StringData.TryGetValue("in__" + name, out string cache);
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
                        module.StringData["in__" + name] = product.Value.Id.Value;
                        return product.Value;
                    }
                }

                module.NumberData.TryRemove("in__" + name, out slimId);
                module.StringData.TryRemove("in__" + name, out cache);
                return null;
            }

            public T Entity<T>(string name)
                where T : class, IEntity
            {
                if (module.NumberData.TryGetValue("in__" + name, out int data))
                {
                    module.Context.EntitiesManager.TryGetEntity(new EntityId(data), out T entity);
                    return entity;
                }
                return default;
            }

            public IProtoWithIcon EntityProtoIconified(string name)
            {
                module.NumberData.TryGetValue("in__" + name, out int slimId);

                if (slimId == 0)
                {
                    return default;
                }

                module.StringData.TryGetValue("in__" + name, out string cache);
                if (!string.IsNullOrEmpty(cache))
                { // try get entity by name and check slimId
                    Option<Proto> product = module.Context.ProtosDb.Get<Proto>(new Mafi.Core.Prototypes.Proto.ID(cache));
                    if (product.HasValue && FixSavedGames.GetPrototypeString(product.Value.Id.Value).RawValue == slimId)
                    {
                        return product.Value as IProtoWithIcon;
                    }
                }
                // else
                { // try get entity by slimId
                    Option<Proto> product = module.Context.ProtosDb.First<Proto>(p => FixSavedGames.GetPrototypeString(p.Id.Value).RawValue == slimId);
                    if (product.HasValue)
                    {
                        module.StringData["in__" + name] = product.Value.Id.Value;
                        return product.Value as IProtoWithIcon;
                    }
                }

                module.NumberData.TryRemove("in__" + name, out slimId);
                return default;
            }
        }
    }
}