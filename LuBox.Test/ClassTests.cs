using System;
using System.Collections.Generic;
using LuBox.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class ClassTests
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
        public void ClassTest()
        {
            _environment.AddClass(typeof(List<object>), "List");
            var list = _luScriptEngine.Evaluate<List<object>>("List()", _environment);
            Assert.IsInstanceOfType(list, typeof(List<object>));
        }

        [TestMethod]
        public void ClassTestParameters()
        {
            _environment.AddClass(typeof(List<object>), "List");
            var list = _luScriptEngine.Evaluate<List<object>>("List({ 1, 2 })", _environment);
            CollectionAssert.AreEqual(new List<object>(new object[] { 1, 2 }), list);
        }
    }
}