using System;
using System.Collections;
using System.Collections.Generic;
using Simple.Data.Extensions;

namespace Simple.Data.Oracle
{
    public class DbDictionary : IDictionary<string, object>
    {
        private readonly IDictionary<string,object> _dict = new Dictionary<string, object>();

        public object this[string key]
        {
            get { return _dict[key.Homogenize()]; }
            set { _dict[key.Homogenize()] = value; }
        }

        public void Add(string key, object value)
        {
            if (DBNull.Value.Equals(value))
                value = null;
            _dict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key.Homogenize());
        }

        #region Passthrough implementation of IDictionary

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            _dict.Add(item);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            _dict.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return _dict.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return _dict.Remove(item);
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return _dict.IsReadOnly; }
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dict.TryGetValue(key, out value);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return _dict.Keys; }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return _dict.Values; }
        }
        #endregion
    }
}