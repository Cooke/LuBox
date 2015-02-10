using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    using System.Collections.Generic;

    [TestClass]
    public class SandboxTests
    {
        private LuScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        [ExpectedException(typeof(LuSandboxException))]
        public void ThrowSandboxExceptionWhenUsingGetType()
        {
            var test = new Test();
            _luScriptEngine.Globals.test = test;
            
            _luScriptEngine.Execute(@"typeName = test:GetType().Name");
        }

        private class Test
        {
        }
    }
}