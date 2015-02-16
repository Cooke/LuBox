using System;
using System.Collections;
using System.Collections.Generic;
using LuBox.Compiler;
using LuBox.Runtime;

namespace LuBox
{
    public class LuEnvironment
    {
        private readonly IDictionary<object, object> _variables = new Dictionary<object, object>();
        private readonly DynamicDictionaryWrapper _variablesDynamic;

        public LuEnvironment()
        {
            _variables["iter"] = new Func<IEnumerable, Func<object>>(x =>
            {
                var enumerator = x.GetEnumerator();
                return (() => enumerator.MoveNext() ? enumerator.Current : null);
            });
            _variablesDynamic = new DynamicDictionaryWrapper(_variables);
        }

        public IDictionary<object, object> Dictionary
        {
            get { return _variables; }
        }

        public dynamic Variables
        {
            get { return _variablesDynamic; }
        }

        public void Set(object key, object value)
        {
            _variables[key] = value;
        }

        public object Get(object key)
        {
            return _variables.ContainsKey(key) ? _variables[key] : null;
        }

        public void SetType(Type type)
        {
            if (type.IsEnum)
            {
                Set(type.Name, new LuEnumWrapper(type));
            }
            else
            {
                throw new ArgumentException("The specified type is not allowed");
            }
        }

        public void Clear()
        {
            _variables.Clear();
        }
    }
}