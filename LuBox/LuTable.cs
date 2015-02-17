using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LuBox.Compiler;
using LuBox.Runtime;

namespace LuBox
{
    public class LuTable
    {
        public static MethodInfo SetMethodInfo = typeof(LuTable).GetMethod("SetField");
        public static MethodInfo GetMethodInfo = typeof(LuTable).GetMethod("GetField");

        private readonly IDictionary<object, object> _fields = new Dictionary<object, object>();
        private readonly DynamicDictionaryWrapper _dynamic;

        public LuTable()
        {
            _fields["iter"] = new Func<IEnumerable, Func<object>>(x =>
            {
                var enumerator = x.GetEnumerator();
                return (() => enumerator.MoveNext() ? enumerator.Current : null);
            });
            _dynamic = new DynamicDictionaryWrapper(_fields);
        }

        public dynamic Dynamic
        {
            get { return _dynamic; }
        }

        public bool HasField(object key)
        {
            return _fields.ContainsKey(key);
        }

        public void SetField(object key, object value)
        {
            _fields[key] = value;
        }

        public object GetField(object key)
        {
            return _fields.ContainsKey(key) ? _fields[key] : null;
        }

        public void AddEnum(Type type)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("The specified type is not allowed");
            }
            
            SetField(type.Name, new LuEnumWrapper(type));
        }
    }
}