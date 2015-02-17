using System;
using LuBox.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class EnumTests
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
        public void EventTest()
        {
            _environment.AddEnum(typeof(Number));
            var o = _luScriptEngine.Evaluate<Number>("Number.One", _environment);
            Assert.AreEqual(Number.One, o);
        }

        private enum Number
        {
            One,
            Two,
            Three
        }
    }
}