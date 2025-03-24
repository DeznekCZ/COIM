using Mafi;
using System;

namespace ProgramableNetwork
{
    public class ReadNumberReference<T>
    {
        private readonly Module.FieldOrInputData data;
        private readonly Func<Fix32, T> getter;
        private readonly Func<T, Fix32> setter;
        private readonly T defaultValue;

        public ReadNumberReference(Module.FieldOrInputData data, Func<Fix32, T> getter, Func<T, Fix32> setter, T defaultValue)
        {
            this.data = data;
            this.getter = getter;
            this.setter = setter;
            this.defaultValue = defaultValue;
        }

        public T this[string name]
        {
            get => this[name, defaultValue];
        }

        public T this[string name, T defaultValue]
        {
            get => getter(data[name, setter(defaultValue)]);
        }
    }
}