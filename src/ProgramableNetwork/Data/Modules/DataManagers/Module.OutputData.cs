using Mafi;

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
        }
    }
}