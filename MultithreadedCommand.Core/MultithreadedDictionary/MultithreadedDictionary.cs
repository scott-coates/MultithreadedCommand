using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.MultithreadedDictionary
{
    public sealed class MultithreadedDictionary<TKey, TValue> : IMultithreadedDictionary<TKey, TValue>
    {
        private static readonly MultithreadedDictionary<TKey, TValue> _instance = new MultithreadedDictionary<TKey, TValue>();

        private object _syncRoot = new object();

        private IDictionary<TKey, TValue> _dictionary { get; set; }

        private MultithreadedDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public static MultithreadedDictionary<TKey, TValue> Instance
        {
            get { return _instance; }
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                _dictionary.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_syncRoot)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_syncRoot)
                {
                    return _dictionary.Keys;
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (_syncRoot)
            {
                return _dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (_syncRoot)
                {
                    return _dictionary.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _dictionary[key];
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    _dictionary[key] = value;
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                _dictionary.Add(item);
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                return _dictionary.Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_syncRoot)
            {
                _dictionary.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _dictionary.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock (_syncRoot)
                {
                    return _dictionary.IsReadOnly;
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
            {
                return _dictionary.Remove(item);
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _dictionary.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _dictionary.GetEnumerator();
            }
        }

        #endregion

        #region IMultithreadedDictionary<TKey,TValue> Members

        public ISynchronizedValue<TValue> SynchronizedValue(TKey Key)
        {
            return new SynchronizedValue<TValue>(_syncRoot, _dictionary[Key]);
        }

        #endregion
    }
}
