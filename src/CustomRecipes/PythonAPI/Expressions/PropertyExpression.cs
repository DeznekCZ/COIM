using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions
{
    public class PropertyExpression : IExpression
    {
        private readonly IExpression expression;
        private readonly string name;
        private object expressionValue;

        public string Path => $"{expression.Path}.{name}";

        public PropertyExpression(IExpression expression, string value)
        {
            this.expression = expression;
            this.name = value;
        }

        public Reference<object> GetReference(IDictionary<string, object> context)
        {
            expressionValue = this.expression.GetValue(context);
            return EvaluateReference(expressionValue);
        }
		public async Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context) {
			expressionValue = await this.expression.GetValueAsync(context);
			return EvaluateReference(expressionValue);
		}

		public object GetValue(IDictionary<string, object> context)
        {
            expressionValue = this.expression.GetValue(context);
            return EvaluateReference(expressionValue).Value;
        }
		public async Task<object> GetValueAsync(IDictionary<string, object> context) {
			expressionValue = await this.expression.GetValueAsync(context);
			return EvaluateReference(expressionValue).Value;
		}

		protected Reference<object> EvaluateReference(object value)
        {
            NullCheck("Can not get property of None {0}");

            if (value is IDictionary<string, object> dict)
            {
                // A read-only map (`config`) exposes constants: reads resolve normally,
                // writes are refused instead of silently rewriting the player's settings.
                Action<object> write = dict.IsReadOnly
                    ? (Action<object>)((v) => throw new InvalidOperationException(
                        $"\"{safePath()}\" is read-only — its values are constants. "
                        + "Assign to a local variable instead."))
                    : ((v) => dict[name] = v);

                if (dict.ContainsKey(name))
                {
                    return new Reference<object>(write, () => dict[name]);
                }

                // Key absent. Fall back to a public instance method of the dictionary's
                // own runtime type so dictionary-backed context objects can expose
                // helpers — `config.get("field", fallback)` on ConfigValues — without
                // those helper names shadowing real entries of the same name.
                MethodInfo[] helpers = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == this.name)
                    .ToArray();

                if (helpers.Length > 0)
                {
                    object helper = MemberCall.Create(value, helpers);
                    return new Reference<object>(write, () => helper);
                }

                // READING an unknown key is a modder error worth naming (the bare
                // Dictionary KeyNotFoundException mentions neither the key nor the
                // object). WRITING one still creates it on a writable map, so
                // `some_map.new_key = v` keeps working.
                return new Reference<object>(
                    write,
                    () => throw new KeyNotFoundException(describeMissingKey(value, dict)));
            }
            else if (value is Type type)
            {
                MemberInfo[] staticMembers = type
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Static);

                if (staticMembers.Length > 0 && staticMembers[0] is PropertyInfo property) {
					return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));
				}
				if (staticMembers.Length > 0 && staticMembers[0] is FieldInfo field) {
					return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));
				}

				if (type.GetNestedType(this.name, BindingFlags.Public) is Type subtype) {
					return new Reference<object>((v) => { }, () => subtype);
				}

				MethodInfo[] staticMethods = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == this.name)
                .ToArray();

                return staticMethods.Length > 0 ? new Reference<object>(
                    (v) => throw new InvalidOperationException($"{type} is sealed, can not set method \"{this.name}\""),
                    () => MemberCall.Create(null, staticMethods)) :
                     throw new KeyNotFoundException($"{type} has no member with name \"{this.name}\"");
            }
            else
            {
                PropertyInfo property = value.GetType().GetProperty(name);
                if (property != null) {
					return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));
				}

				// Look for explicitly implemented interface properties
                foreach (var interfaceType in value.GetType().GetInterfaces())
                {
                    PropertyInfo interfaceProperty = interfaceType.GetProperty(name);
                    if (interfaceProperty != null)
                    {
                        return new Reference<object>(
                            (v) => interfaceProperty.SetValue(value, v),
                            () => interfaceProperty.GetValue(value));
                    }
                }

                FieldInfo field = value.GetType().GetField(name);
                if (field != null) {
					return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));
				}

				MethodInfo[] instanceMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == this.name)
                    .ToArray();

                var member = instanceMethods.Length > 0 ? MemberCall.Create(value, instanceMethods) :
                     throw new KeyNotFoundException($"{value.GetType()} has no member with name \"{this.name}\"");
                return new Reference<object>(
                    (v) => throw new InvalidOperationException($"{value.GetType()} is sealed, can not set method \"{this.name}\""),
                    () => member);
            }
        }

        // Error text for a `.member` read that misses on a dictionary-backed value.
        // Shared with the subscript form (`config["nope"]`) so both report the same
        // thing — see MissingMember.
        private string describeMissingKey(object owner, IDictionary<string, object> dict)
        {
            return MissingMember.Describe(owner, dict, this.name, safePath());
        }

        // Path throws NotImplementedException for expression kinds that have no
        // source path (a call result, an index, …); the error message must not.
        private string safePath()
        {
            try
            {
                return Path;
            }
            catch (NotImplementedException)
            {
                return this.name;
            }
        }

        protected object NullCheck(string format)
        {
            return expressionValue is null ? throw new NullReferenceException(string.Format(format, this.expression)) : expressionValue;
        }
    }
}