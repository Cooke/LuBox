using System;
using System.Collections.Generic;

namespace LuBox
{
    public class LuCompileException : Exception
    {
        private readonly IEnumerable<LuCompileMessage> _messages;

        public LuCompileException(IEnumerable<LuCompileMessage> messages)
        {
            _messages = messages;
        }

        public IEnumerable<LuCompileMessage> Messages
        {
            get { return _messages; }
        }
    }
}