using System;

namespace LuBox
{
    public class LuSandboxException : Exception
    {
        public LuSandboxException(string message)
            : base(message)
        {
        }
    }
}
