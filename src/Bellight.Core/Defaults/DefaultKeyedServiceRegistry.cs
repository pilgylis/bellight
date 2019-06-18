using System;
using System.Collections.Generic;

namespace Bellight.Core.Defaults
{
    public class DefaultKeyedServiceRegistry : IKeyedServiceRegistry
    {
        private readonly IDictionary<string, Type> _dictionary = new Dictionary<string, Type>();
        public void Add(string key, Type type)
        {
            if (_dictionary.ContainsKey(key))
            {
                return;
            }

            _dictionary.Add(key, type);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IDictionary<string, Type> GetDictionary()
        {
            return _dictionary;
        }
    }
}
