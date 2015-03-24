using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LuBox.Compiler;

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
                },
                {
                    "ipairs", new Func<LuTable, Func<object, object, object>>(x =>
                    {
                        var keys = x.GetKeys().ToArray();
                        var index = 0;
                        return ((v, s) => index < keys.Length ? new LuMultiResult(new[] { index + 1, x.GetField(keys[index++]) }) : null);
                    })
                },
                {
                    "pairs", new Func<LuTable, Func<object, object, object>>(x =>
                    {
                        var keys = x.GetKeys().ToArray();
                        var index = 0;
                        return ((v, s) => index < keys.Length ? new LuMultiResult(new[] { keys[index], x.GetField(keys[index++]) }) : null);
                    })
                }
            };
            return new LuTable(dictionary);
        }
    }
}
