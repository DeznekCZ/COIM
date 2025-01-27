using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;

namespace ProgramableNetwork.Python
{
    internal class PropertyExpression : IExpression
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

                if (staticMembers[0] is PropertyInfo property)
                    return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));
                if (staticMembers[0] is FieldInfo field)
                    return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));

                MethodInfo[] staticMethods = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == this.name)
                .ToArray();

                return staticMethods.Length > 0 ? new Reference<object>(
                    (v) => throw new InvalidOperationException($"{type} is sealed, can not set method \"{this.name}\""),
                    () => MemberCall.Create(null, staticMethods)) :
                     throw new KeyNotFoundException($"{type} has no member with name \"{this.name}\"");
            }
            else if (value is ModuleWrapper module)
            {
                MemberInfo[] instanceMembers = value.GetType()
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Instance);
                MemberInfo[] staticMembers = value.GetType()
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Static);

                if (instanceMembers.Length > 0)
                {
                    if (instanceMembers[0] is PropertyInfo property)
                        return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));
                    if (instanceMembers[0] is FieldInfo field)
                        return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));
                }
                else if (staticMembers.Length > 0)
                {
                    if (staticMembers[0] is PropertyInfo property)
                        return new Reference<object>((v) => property.SetValue(null, v), () => property.GetValue(null));
                    if (staticMembers[0] is FieldInfo field)
                        return new Reference<object>((v) => field.SetValue(null, v), () => field.GetValue(null));
                }

                MethodInfo[] instanceMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == this.name)
                    .ToArray();
                MethodInfo[] staticMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == this.name)
                    .ToArray();

                if (instanceMethods.Length == 0 && staticMethods.Length == 0
                    && module.@class.classContext.TryGetValue(this.name, out object f))
                {
                    if (f is Method function)
                    {
                        return new Reference<object>((v) =>
                            {
                                module.@class.classContext[name] = v;
                            },
                            () =>
                            {
                                function.Self = module;
                                return function;
                            });
                    }
                    return new Reference<object>(
                        (v) => module.@class.classContext[name] = v,
                        () => f);
                }

                var member = instanceMethods.Length > 0 ? MemberCall.Create(value, instanceMethods) :
                     staticMethods.Length > 0 ? MemberCall.Create(null, staticMethods) :
                     throw new KeyNotFoundException($"{module.@class.name} has no member with name \"{this.name}\"");
                return new Reference<object>(
                    (v) => throw new InvalidOperationException($"{module.@class.name} is sealed, can not set method \"{this.name}\""),
                    () => member);
            }
            else
            {
                MemberInfo[] instanceMembers = value.GetType()
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Instance);
                MemberInfo[] staticMembers = value.GetType()
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Static);

                if (instanceMembers.Length > 0)
                {
                    if (instanceMembers[0] is PropertyInfo property)
                        return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));
                    if (instanceMembers[0] is FieldInfo field)
                        return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));
                }
                else if (staticMembers.Length > 0)
                {
                    if (staticMembers[0] is PropertyInfo property)
                        return new Reference<object>((v) => property.SetValue(value, v), () => property.GetValue(value));
                    if (staticMembers[0] is FieldInfo field)
                        return new Reference<object>((v) => field.SetValue(value, v), () => field.GetValue(value));
                }

                MethodInfo[] instanceMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == this.name)
                    .ToArray();
                MethodInfo[] staticMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == this.name)
                    .ToArray();

                var member = instanceMethods.Length > 0 ? MemberCall.Create(value, instanceMethods) :
                     staticMethods.Length > 0 ? MemberCall.Create(null, staticMethods) :
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