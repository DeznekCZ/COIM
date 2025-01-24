using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;

namespace ProgramableNetwork.Python
{
    internal class PropertyExpression : AUnaryOperatorExpression
    {
        private string name;

        public override string Path => $"{base.expression.Path}.{name}";

        public PropertyExpression(IExpression expression, string value)
            : base(expression)
        {
            this.name = value;
        }

        protected override object Evaluate(object value)
        {
            NullCheck("Can not get property of None {0}");

            if (value is IDictionary<string, object> dict)
            {
                return new Reference<object>(null, () => dict[this.name]);
            }
            else if (value is System.Type type)
            {
                MemberInfo[] staticMembers = type
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Static);

                if (staticMembers[0] is PropertyInfo property)
                    return property.GetValue(value);
                if (staticMembers[0] is FieldInfo field)
                    return field.GetValue(value);

                MethodInfo[] staticMethods = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == this.name)
                .ToArray();

                return staticMethods.Length > 0 ? MemberCall.Create(null, staticMethods) :
                     throw new KeyNotFoundException($"{type} has no member with name {this.name}");
            }
            else if (value is ModuleWrapper module)
            {
                if (module.@class.classContext.TryGetValue(this.name, out object f))
                {
                    if (f is Function function)
                    {
                        function.Self = module;
                        return function;
                    }
                    return f;
                }

                MemberInfo[] instanceMembers = value.GetType()
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Instance);
                MemberInfo[] staticMembers = value.GetType()
                    .GetMember(this.name, BindingFlags.Public | BindingFlags.Static);

                if (instanceMembers.Length > 0)
                {
                    if (instanceMembers[0] is PropertyInfo property)
                        return property.GetValue(value);
                    if (instanceMembers[0] is FieldInfo field)
                        return field.GetValue(value);
                }
                else if (staticMembers.Length > 0)
                {
                    if (staticMembers[0] is PropertyInfo property)
                        return property.GetValue(value);
                    if (staticMembers[0] is FieldInfo field)
                        return field.GetValue(value);
                }

                MethodInfo[] instanceMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == this.name)
                    .ToArray();
                MethodInfo[] staticMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == this.name)
                    .ToArray();

                return instanceMethods.Length > 0 ? MemberCall.Create(value, instanceMethods) :
                     staticMethods.Length > 0 ? MemberCall.Create(null, staticMethods) :
                     throw new KeyNotFoundException($"{module.@class.name} has no member with name {this.name}");
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
                        return property.GetValue(value);
                    if (instanceMembers[0] is FieldInfo field)
                        return field.GetValue(value);
                }
                else if (staticMembers.Length > 0)
                {
                    if (staticMembers[0] is PropertyInfo property)
                        return property.GetValue(value);
                    if (staticMembers[0] is FieldInfo field)
                        return field.GetValue(value);
                }

                MethodInfo[] instanceMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == this.name)
                    .ToArray();
                MethodInfo[] staticMethods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == this.name)
                    .ToArray();

                return instanceMethods.Length > 0 ? MemberCall.Create(value, instanceMethods) :
                     staticMethods.Length > 0 ? MemberCall.Create(null, staticMethods) :
                     throw new KeyNotFoundException($"{value.GetType()} has no member with name {this.name}");
            }
        }
    }
}