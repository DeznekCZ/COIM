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
        private readonly bool empty;
        private object value;
        private bool found;
        private T casted;

        private readonly List<Type> types;

        public AnyArgument(string name, object value, bool empty = false)
        {
            this.name = name;
            this.value = value;
            this.empty = empty;
            if (value is T casted)
            {
                this.casted = casted;
                this.found = !empty;
            }
            this.types = new List<Type>() { typeof(T) };
        }

        public AnyArgument<T> When<D>(Func<D, T> converter)
        {
            this.types.Add(typeof(D));
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
                throw new ArgumentException("Argument does not contain any matching type: "
                    + string.Join(", ", types.Select(t => t.FullName.Replace("+", ".")).ToArray())
                    + " but received: " + value?.GetType()?.FullName?.Replace("+", ".") ?? "None");
            return casted;
        }

        public bool ElseNotExists(out T value)
        {
            value = casted;
            return found;
        }

        public T ElseDefault(T value)
        {
            if (!found && !empty && !(this.value is null))
                throw new ArgumentException("Argument does not contain any matching type: "
                    + string.Join(", ", types.Select(t => t.FullName.Replace("+", ".")).ToArray())
                    + " but received: " + this.value?.GetType()?.FullName?.Replace("+", ".") ?? "None");

            return found ? casted : value;
        }
    }
}