using Mafi;
using System;

namespace ProgramableNetwork
{
    public class Reference
    {
        public Fix32 Value
        {
            get => getter();
            set => setter(value);
        }

        private readonly Action<Fix32> setter;
        private readonly Func<Fix32> getter;

        public Reference(Action<Fix32> setter, Func<Fix32> getter)
        {
            this.setter = setter;
            this.getter = getter;
        }
    }
}