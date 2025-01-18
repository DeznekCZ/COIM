using Mafi.Unity.UiToolkit;
using System.Collections.Generic;
using System.Linq;

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

        public dynamic GetValue(IDictionary<string, dynamic> context)
        {
            if (names.Count == 1)
            {
                return context.TryGetValue(names[0].value, out dynamic value) ? value : throw new KeyNotFoundException(names[0].ToString());
            }

            throw new System.NotImplementedException();
        }
    }
}