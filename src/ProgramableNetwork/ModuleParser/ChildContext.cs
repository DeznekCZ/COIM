using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class ChildContext<K,T> : IDictionary<K, T>
    {
        private Dictionary<K, T> scope;
        private IDictionary<K, T> parent;

        public ChildContext(IDictionary<K, T> context)
        {
            parent = context;
            scope = new Dictionary<K, T>();
        }

        public T this[K key] {
            get => scope.TryGetValue(key, out T value) ? value : parent[key];
            set => scope[key] = value;
        }

        public ICollection<K> Keys => scope.Keys.Concat(parent.Keys).Distinct().ToList();

        public ICollection<T> Values => Keys.Select(k => this[k]).ToList();

        public int Count => throw new System.NotImplementedException();

        public bool IsReadOnly => throw new System.NotImplementedException();

        public void Add(K key, T value)
        {
            scope.Add(key, value);
        }

        public void Add(KeyValuePair<K, T> item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(KeyValuePair<K, T> item)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsKey(K key)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<K, T>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<K, T>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(K key)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<K, T> item)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(K key, out T value)
        {
            return scope.TryGetValue(key, out value) || parent.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}