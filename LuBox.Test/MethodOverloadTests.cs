using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class MethodOverloadTests
    {
        private LuScriptEngine _engine;
        private LuTable _environment;

        [TestInitialize]
        public void Initialize()
        {
            _engine = new LuScriptEngine();
            _environment = new LuTable();
        }

        [TestMethod]
        public void ShallInitializeParamsArrayWhenArgumentsForItAreMissing()
        {
            var test = new Test();
            _environment.Dynamic.test = test;
            var result = _engine.Evaluate<string>("test:CallParams('hello')", _environment);
            Assert.AreEqual("string params object", result);
        }

        [TestMethod]
        public void ShallPickParamsOverload()
        {
            var test = new Test();
            _environment.Dynamic.test = test;
            var result = _engine.Evaluate<string>("test:Call('hello', 123)", _environment);
            Assert.AreEqual("string params object", result);
        }

        [TestMethod]
        public void ShallCallIntWithDouble()
        {
            var test = new Test();
            _environment.Dynamic.test = test;
            var result = _engine.Evaluate<int>("test:CallInt(8.3)", _environment);
            Assert.AreEqual(8, result);
        }

        public class Test
        {
            public int CallInt(int integer)
            {
                return integer;
            }

            public string Call(string messages)
            {
                return "string";
            }

            public string Call(string messages, params object[] args)
            {
                Assert.IsNotNull(args);
                return "string params object";
            }

            public string CallParams(string messages, params object[] args)
            {
                Assert.IsNotNull(args);
                return "string params object";
            }
        }
    }
}
