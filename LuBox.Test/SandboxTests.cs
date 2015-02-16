using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    using System.Collections.Generic;

    [TestClass]
    public class SandboxTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuEnvironment _environment;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _environment = new LuEnvironment();
        }

        [TestMethod]
        [ExpectedException(typeof (LuSandboxException))]
        public void ThrowSandboxExceptionWhenUsingGetType()
        {
            var test = new Test();
            _environment.Variables.test = test;

            _luScriptEngine.Execute(@"typeName = test:GetType().Name", _environment);

        }

        [TestMethodAttribute]
        [ExpectedException(typeof(LuSandboxException))]
        public void ShallThrowWhenUsingTypeProperty()
        {
            var test = new Test();
            _environment.Variables.test = test;
            
            _luScriptEngine.Execute(@"type = test.Prop", _environment);
        }

        private class Test
        {
            public Type Prop { get { return GetType(); } }
        }
    }
}