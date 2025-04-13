using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomRecipes.Python
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

        public object GetValue(IDictionary<string, object> context)
        {
            expressionValue = this.expression.GetValue(context);
            return EvaluateReference(expressionValue).Value;
        }

        protected Reference<object> EvaluateReference(object value)
        {
            NullCheck("Can not get property of None {0}");

            if (value is IDictionary<string, object> dict)
            {
                return new Reference<object>((v) => dict[name] = v, () => dict[name]);
            }
            else if (value is Type type)
            {
                MemberInfo[] staticMembers = type
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Static);

                if (staticMembers.Length > 0 && staticMembers[0] is PropertyInfo property)
                    return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));
                if (staticMembers.Length > 0 && staticMembers[0] is FieldInfo field)
                    return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));

                if (type.GetNestedType(this.name, BindingFlags.Public) is Type subtype)
                    return new Reference<object>((v) => { }, () => subtype);

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
                if (property != null)
                    return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));

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
                if (field != null)
                    return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));

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

        protected object NullCheck(string format)
        {
            return expressionValue is null ? throw new NullReferenceException(string.Format(format, this.expression)) : expressionValue;
        }
    }
}