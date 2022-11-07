using System;
using System.Collections.Generic;
using System.Text;

namespace EventAggregator
{
    class StringMap<TValue> : IStringMap<TValue>
        where TValue : class
    {
        Dictionary<string, TValue> collection = new Dictionary<string, TValue>();
        public int Count => collection.Count;

        public TValue DefaultValue { get; set; }

        public bool AddElement(string key, TValue value)
        {
            if (collection.ContainsKey(key))
            {
                collection[key] = value;
                return true;
            }

            collection.Add(key, value);
            return false;
        }

        public TValue GetValue(string key)
        {
            if (collection.ContainsKey(key))
                return collection[key];

            return DefaultValue;
        }

        public bool RemoveElement(string key)
        {
            if (key == null)
                throw new ArgumentNullException();

            if (String.IsNullOrEmpty(key))
                throw new ArgumentException();

            return collection.Remove(key);
        }
    }
}
