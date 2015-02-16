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

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        public void NumericFor()
        {
            _luScriptEngine.Globals.counter = 0;
            _luScriptEngine.Execute("for i=1, 10, 1 do counter = counter + 1 end");
            Assert.AreEqual(10, _luScriptEngine.Globals.counter);
        }

        [TestMethod]
        public void NumericForUseVariable()
        {
            _luScriptEngine.Globals.counter = 0;
            _luScriptEngine.Execute("for i=0, 9, 1 do counter = counter + i end");
            Assert.AreEqual(45, _luScriptEngine.Globals.counter);
        }

        [TestMethod]
        public void NumericForOverCollection()
        {
            _luScriptEngine.Globals.counter = 0;
            _luScriptEngine.Globals.collection = new int[10];
            _luScriptEngine.Execute("for i=0, collection.Length-1, 1 do counter = counter + i end");
            Assert.AreEqual(45, _luScriptEngine.Globals.counter);
        }

        [TestMethod]
        public void NumericForOverCollectionWithIndex()
        {
            _luScriptEngine.Globals.sum = 0;
            _luScriptEngine.Globals.collection = new[] { 3, 3, 3};
            _luScriptEngine.Execute("for i=0, collection.Length-1, 1 do sum = sum + collection[i] end");
            Assert.AreEqual(9, _luScriptEngine.Globals.sum);
        }

        [TestMethod]
        public void ForIn()
        {
            _luScriptEngine.Globals.counter = 0;
            _luScriptEngine.Globals.sum = 0;
            _luScriptEngine.Execute("function iter() if counter == 3 then return nil else counter = counter + 1; return counter; end end " +
                                    "for i in iter do sum = sum + 1 end");
            Assert.AreEqual(3, _luScriptEngine.Globals.sum);
        }

        [TestMethod]
        public void ForInCollection()
        {
            _luScriptEngine.Globals.sum = 0;
            _luScriptEngine.Globals.collection = new [] { 3, 4, 5};
            _luScriptEngine.Execute("for i in iter(collection) do sum = sum + i end");
            Assert.AreEqual(12, _luScriptEngine.Globals.sum);
        }
    }
}
