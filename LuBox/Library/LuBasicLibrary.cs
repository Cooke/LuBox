using System;
using System.Collections;
using System.Collections.Generic;

namespace LuBox.Library
{
    public static class LuBasicLibrary
    {
        public static LuTable Create()
        {
            var dictionary = new Dictionary<object, object>
            {
                {
                    "iter", new Func<IEnumerable, Func<object, object, object>>(x =>
                    {
                        var enumerator = x.GetEnumerator();
                        return ((v, s) => enumerator.MoveNext() ? enumerator.Current : null);
                    })
                }
            };
            return new LuTable(dictionary);
        }
    }
}
