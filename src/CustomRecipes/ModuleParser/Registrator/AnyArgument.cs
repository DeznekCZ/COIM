using Mafi;
using Mafi.Core.Factory.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomRecipes.ModuleParser.Registrator
{
    public class AnyArgument<T>
    {
        private readonly string name;
        private object value;
        private bool found;
        private T casted;

        private readonly List<Type> types;

        public AnyArgument(string name, object value, bool empty = false)
        {
            this.name = name;
            this.value = value;
            if (value is T casted)
            {
                this.casted = casted;
                this.found = !empty;
            }
            this.types = new List<Type>() { typeof(T) };
        }

        public AnyArgument<T> When<D>(Func<D, T> converter)
        {
            if (!found && value is D d)
            {
                casted = converter(d);
                found = true;
            }
            return this;
        }

        public T ElseRequiredThrow()
        {
            if (!found)
            {
                throw new ArgumentException("Argument does not contain any matching type: "
                    + string.Join(", ", types.Select(t => t.FullName.Replace("+", "."))
                    + " but received: " + value?.GetType()?.FullName?.Replace("+", ".") ?? "None"));
            }
            return casted;
        }

        public bool ElseNotExists(out T value)
        {
            value = casted;
            return found;
        }

        public T ElseDefault(T value)
        {
            return found ? casted : value;
        }
    }
}