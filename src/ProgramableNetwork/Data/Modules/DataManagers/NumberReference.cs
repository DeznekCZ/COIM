using Mafi;
using System;

namespace ProgramableNetwork
{
    public class NumberReference<T> : IReference<T>
        where T : new()
    {
        private readonly IReference<Fix32> data;
        private readonly Func<T, Fix32> setter;
        private readonly Func<Fix32, T> getter;
        private readonly T defaultValue;

        public NumberReference(IReference<Fix32> data, Func<T, Fix32> setter, Func<Fix32, T> getter, T defaultValue)
        {
            this.data = data;
            this.setter = setter;
            this.getter = getter;
            this.defaultValue = defaultValue;
        }

        public T this[string name]
        {
            get => this[name, defaultValue];
            set => data[name] = setter(value);
        }

        public T this[string name, T defaultValue = default]
        {
            get => getter(data[name, setter(defaultValue)]);
        }
    }
}