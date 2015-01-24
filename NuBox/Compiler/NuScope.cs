using System.Collections.Generic;

namespace NuBox.Compiler
{
    public class NuScope
    {
        private readonly IDictionary<object, object> _env;

        public NuScope(IDictionary<object, object> env)
        {
            _env = env;
        }

        public object Get(string key)
        {
            return _env[key];
        }
    }
}