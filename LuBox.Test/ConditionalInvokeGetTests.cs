using System;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class ConditionalInvokeGetTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuTable _environment;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _environment = new LuTable();
        }

        [TestMethod]
        public void ShallReturnValueWhenNotNullColon()
        {
            _environment.Dynamic.field1 = "hello";
            var field1 = _luScriptEngine.Evaluate<string>("field1?:ToString()", _environment);
            Assert.AreEqual("hello", field1);
        }

        [TestMethod]
        public void ShallReturnNullWhenNullColon()
        {
            var field1 = _luScriptEngine.Evaluate<string>("field1?:ToString()", _environment);
            Assert.IsNull(field1);
        }

        [TestMethod]
        public void ShallReturnValueWhenNotNullDot()
        {
            _environment.Dynamic.field1 = "hello";
            var field1 = _luScriptEngine.Evaluate<string>("field1?.ToString()", _environment);
            Assert.AreEqual("hello", field1);
        }

        [TestMethod]
        public void ShallReturnNullWhenNullDot()
        {
            var field1 = _luScriptEngine.Evaluate<string>("field1?.ToString()", _environment);
            Assert.IsNull(field1);
        }
    }
}