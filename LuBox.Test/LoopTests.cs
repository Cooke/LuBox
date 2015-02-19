using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class LoopTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuTable _environment;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _environment = _luScriptEngine.CreateStandardEnvironment();
        }

        [TestMethod]
        public void NumericFor()
        {
            _environment.Dynamic.counter = 0;
            _luScriptEngine.Execute("for i=1, 10, 1 do counter = counter + 1 end", _environment);
            Assert.AreEqual(10, _environment.Dynamic.counter);
        }

        [TestMethod]
        public void NumericForUseVariable()
        {
            _environment.Dynamic.counter = 0;
            _luScriptEngine.Execute("for i=0, 9, 1 do counter = counter + i end", _environment);
            Assert.AreEqual(45, _environment.Dynamic.counter);
        }

        [TestMethod]
        public void NumericForOverCollection()
        {
            _environment.Dynamic.counter = 0;
            _environment.Dynamic.collection = new int[10];
            _luScriptEngine.Execute("for i=0, collection.Length-1, 1 do counter = counter + i end", _environment);
            Assert.AreEqual(45, _environment.Dynamic.counter);
        }

        [TestMethod]
        public void NumericForOverCollectionWithIndex()
        {
            _environment.Dynamic.sum = 0;
            _environment.Dynamic.collection = new[] { 3, 3, 3};
            _luScriptEngine.Execute("for i=0, collection.Length-1, 1 do sum = sum + collection[i] end", _environment);
            Assert.AreEqual(9, _environment.Dynamic.sum);
        }

        [TestMethod]
        public void ForIn()
        {
            _environment.Dynamic.counter = 0;
            _environment.Dynamic.sum = 0;
            _luScriptEngine.Execute("function iter() if counter == 3 then return nil else counter = counter + 1; return counter; end end " +
                                    "for i in iter do sum = sum + 1 end", _environment);
            Assert.AreEqual(3, _environment.Dynamic.sum);
        }

        [TestMethod]
        public void ForInCollection()
        {
            _environment.Dynamic.sum = 0;
            _environment.Dynamic.collection = new [] { 3, 4, 5};
            _luScriptEngine.Execute("for i in iter(collection) do sum = sum + i end", _environment);
            Assert.AreEqual(12, _environment.Dynamic.sum);
        }
    }
}
