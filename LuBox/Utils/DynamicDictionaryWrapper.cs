using System.Collections.Generic;
using System.Dynamic;

namespace LuBox.Compiler
{
    internal class DynamicDictionaryWrapper : DynamicObject
    {
        private readonly IDictionary<object, object> _dictionary;

        public DynamicDictionaryWrapper(IDictionary<object, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dictionary.TryGetValue(binder.Name, out result);
        }
    }
}