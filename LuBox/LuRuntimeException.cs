using System;

namespace LuBox
{
    public class LuRuntimeException : Exception
    {
        public LuRuntimeException(string message) : base(message)
        {
        }
    }
}