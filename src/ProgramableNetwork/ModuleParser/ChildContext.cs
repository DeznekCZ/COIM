using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class ChildContext : IDictionary<string, object>
    {
        private Dictionary<string, object> scope;
        private IDictionary<string, object> parent;

        public ChildContext(IDictionary<string, object> context)
        {
            parent = context;
            scope = new Dictionary<string, object>();
        }

        public object this[string key] {
            get
            {
                if (key == "__return__")
                    return scope.TryGetValue(key, out object value) ? value : null;
                else
                    return scope.TryGetValue(key, out object value) ? value : parent[key];
            }
            set => scope[key] = value;
        }

        public ICollection<string> Keys => scope.Keys.Concat(parent.Keys).Distinct().ToList();

        public ICollection<object> Values => Keys.Select(k => this[k]).ToList();

        public int Count => throw new System.NotImplementedException();

        public bool IsReadOnly => throw new System.NotImplementedException();

        public void Add(string key, object value)
        {
            scope.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            return scope.TryGetValue(key, out value) || parent.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}