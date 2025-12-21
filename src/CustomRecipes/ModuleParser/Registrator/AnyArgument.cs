using Mafi;
using Mafi.Core.Factory.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomAssets.ModuleParser.Registrator
{
    public class AnyArgument<TTargetType>
    {
        private readonly string name;
        private readonly bool empty;
        private object value;
        private bool found;
        private TTargetType casted;

        private readonly List<Type> types;

        public AnyArgument(string name, object value, bool empty = false)
        {
            this.name = name;
            this.value = value;
            this.empty = empty;
            if (value is TTargetType casted)
            {
                this.casted = casted;
                this.found = !empty;
            }
            this.types = new List<Type>() { typeof(TTargetType) };
        }

        public AnyArgument<TTargetType> When<TConvertFrom>(Func<TConvertFrom, TTargetType> converter)
        {
            this.types.Add(typeof(TConvertFrom));
			if (found || value is not TConvertFrom d) {
				return this;
			}
			casted = converter(d);
			found = true;
			return this;
        }

        public TTargetType ElseRequiredThrow()
        {
            if (found == false) {
				throw new ArgumentException("Argument does not contain any matching type: "
					+ string.Join(", ", types.Select(t => t.FullName.Replace("+", ".")).ToArray())
					+ " but received: " + value?.GetType()?.FullName?.Replace("+", ".") ?? "None");
			}
			return casted;
        }

        public bool WhenExists(out TTargetType value)
        {
            value = casted;
            return found;
        }

        public TTargetType ElseDefault(TTargetType value)
        {
			if (found || empty || this.value is null) {
				return found ? casted : value;
			}
			throw new ArgumentException("Argument does not contain any matching type: "
				+ string.Join(", ", types.Select(t => t.FullName?.Replace("+", ".")).ToArray())
				+ " but received: " + this.value?.GetType()?.FullName?.Replace("+", ".") ?? "None");

		}

        public TTargetType ElseNull() {
			return found ? casted : default;
		}
    }
}