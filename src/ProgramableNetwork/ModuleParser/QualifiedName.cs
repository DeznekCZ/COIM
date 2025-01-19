using Mafi.Unity.UiToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProgramableNetwork.Python
{
    public class QualifiedName : IExpression
    {
        public readonly List<Token> names = new List<Token>();

        public string Concat => string.Join(".", names.Select(n => n.value));

        public string StringValue => throw new System.NotImplementedException("could not evaluate");

        public int IntValue => throw new System.NotImplementedException("could not evaluate");

        public long LongValue => throw new System.NotImplementedException("could not evaluate");

        public bool BooleanValue => throw new System.NotImplementedException("could not evaluate");

        public Reference<dynamic> GetReference(IDictionary<string, dynamic> context)
        {
            if (names.Count == 1)
            {
                return new Reference<dynamic>(
                    (v) => context[names[0].value] = v,
                    () => context[names[0].value]);
            }

            throw new System.NotImplementedException();
        }

        public object GetValue(IDictionary<string, object> context)
        {
            if (names.Count == 1)
            {
                return context.TryGetValue(names[0].value, out object value) ? value : throw new KeyNotFoundException(names[0].ToString());
            }

            if (!context.TryGetValue(names[0].value, out object result))
                throw new KeyNotFoundException(names[0].ToString());

            for (int i = 1; i < names.Count; i++)
            {
                Type type = result is Type t ? t : result?.GetType();
                if (type == null)
                {
                    throw new NullReferenceException($"Value got by token is null: {names[i - 1]}");
                }

                if (result is IDictionary<string, object> idict)
                {
                    result = idict.TryGetValue(names[i].value, out result) ? result : throw new KeyNotFoundException(names[i].ToString());
                }
                else if (type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Any(m => m.Name == "TryGetValue"))
                {
                    // TODO reflective call to dictionary
                    var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).First(m => m.Name == "TryGetValue");
                    Type refType = method.GetParameters()[1].ParameterType;
                    object[] args = new object[] { names[i].value, Convert.ChangeType(null, refType) };
                    result = method.Invoke(context, args);
                    Console.WriteLine(method.Name);
                }
                else
                {
                    MethodInfo method = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).FirstOrDefault(m => m.Name == names[i].value);
                    PropertyInfo property = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).FirstOrDefault(m => m.Name == names[i].value);
                    FieldInfo field = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).FirstOrDefault(m => m.Name == names[i].value);

                    if (method != null)
                    {
                        object refrerence = result;
                        if (method.ReturnType != typeof(void))
                        {
                            result = new Constructor((args) =>
                            {
                                return method.Invoke(refrerence, args);
                            });
                        }
                        else
                        {
                            result = new Constructor((args) =>
                            {
                                method.Invoke(refrerence, args);
                                return null;
                            });
                        }
                        continue;
                    }

                    if (property != null)
                    {
                        if (result is Type)
                            result = property.GetValue(null);
                        else
                            result = property.GetValue(result);
                        continue;
                    }

                    if (field != null)
                    {
                        if (result is Type)
                            result = field.GetValue(null);
                        else
                            result = field.GetValue(result);
                        continue;
                    }

                    throw new System.MissingMemberException($"{type}.{names[i].value} at {names[i]}");
                }
            }
            return result;
        }
    }
}