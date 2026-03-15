using Mafi;
using System;

namespace ProgramableNetwork
{
    public class Reference
    {
        public Fix32 Value {
            get => getter();
            set => setter(value);
        }

		public string StringValue {
			get => getterS();
			set => setterS(value);
		}

		private readonly Action<Fix32> setter;
        private readonly Func<Fix32> getter;

		private readonly Action<string> setterS;
        private readonly Func<string> getterS;

        public Reference(Action<Fix32> setter, Func<Fix32> getter, Action<string> setterS, Func<string> getterS)
        {
            this.setter = setter;
            this.getter = getter;
            this.setterS = setterS;
            this.getterS = getterS;
        }
    }

    public class Reference<T>
    {
        public T Value
        {
            get => getter();
            set => setter(value);
        }

        private readonly Action<T> setter;
        private readonly Func<T> getter;

        public Reference(Action<T> setter, Func<T> getter)
        {
            this.setter = setter;
            this.getter = getter;
        }
    }
}