using System.Collections.Generic;

namespace LuBox.Runtime
{
    internal class Scope
    {
        private readonly IDictionary<object, object> _env;

        public Scope(IDictionary<object, object> env)
        {
            _env = env;
        }

        public object Get(string key)
        {
            return _env[key];
        }
    }
}