using System;
using System.Collections.Generic;
using System.Linq;

namespace ConvertApiDotNet
{
    public class ParamDictionary
    {
        private readonly Dictionary<string, List<object>> _dictionary;

        public ParamDictionary()
        {
            _dictionary = new Dictionary<string, List<object>>();
        }

        //Check for duplicate string and add S at the end of parameter
        public Dictionary<string, object> Get()
        {
            var dic = new Dictionary<string, object>();
            foreach (var keyValuePair in _dictionary)
            {
                if (keyValuePair.Value.Count == 1)
                    dic.Add(keyValuePair.Key, keyValuePair.Value[0]);
                else
                {
                    for (var index = 0; index < keyValuePair.Value.Count; index++)
                    {
                        string name;
                        if (!keyValuePair.Key.EndsWith("s"))
                            name = keyValuePair.Key + "s";
                        else
                            name = keyValuePair.Key;
                        dic.Add(name + "[" + index + "]", keyValuePair.Value[index]);
                    }
                }
            }

            return dic;
        }

        public string Find(string key)
        {
            return _dictionary.FirstOrDefault(w => string.Equals(w.Key, key, StringComparison.OrdinalIgnoreCase)).Value?[0]?.ToString();
        }

        public void Add(string key, object value)
        {
            var keyToAdd = key.ToLower();

            if (!_dictionary.ContainsKey(keyToAdd))
            {
                _dictionary.Add(keyToAdd, new List<object> {value});
            }
            else
            {
                _dictionary[keyToAdd].Add(value);
            }
        }
    }
}