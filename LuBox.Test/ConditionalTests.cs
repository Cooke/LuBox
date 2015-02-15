using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class ConditionalTests
    {
        private LuScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        public void IfTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if true then result = 1 end");
            Assert.AreEqual(1, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void IfNegativeTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if false then result = 1 end");
            Assert.AreEqual(0, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void IfThenElseIfTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if true then result = 1 elseif true result = 2 end");
            Assert.AreEqual(1, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void IfThenElseIfNegativeTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if false then result = 1 elseif true result = 2 end");
            Assert.AreEqual(2, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void IfThenElseIfNegativeNegativeTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if false then result = 1 elseif false result = 2 end");
            Assert.AreEqual(0, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void IfThenTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if true then result = 1 else result = 2 end");
            Assert.AreEqual(1, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void IfThenNegativeTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if false then result = 1 else result = 2 end");
            Assert.AreEqual(2, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void IfThenElseIfElseTest()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if false then result = 1 elseif false result = 2 else result = 3 end");
            Assert.AreEqual(3, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void NumberShallBeTrue()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if 1 then result = 1 end");
            Assert.AreEqual(1, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void NumberZeroShallBeTrueAccordingToLuaManual()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if 0 then result = 1 end");
            Assert.AreEqual(1, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void NullShallBeFalse()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Execute("if undefinedVar then result = 1 end");
            Assert.AreEqual(0, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void TrueVariableShallBeTrue()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Globals.var = true;
            _luScriptEngine.Execute("if var then result = 1 end");
            Assert.AreEqual(1, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void FlaseVariableShallBeFalse()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Globals.var = false;
            _luScriptEngine.Execute("if var then result = 1 end");
            Assert.AreEqual(0, _luScriptEngine.Globals.result);
        }

        [TestMethod]
        public void StringVariableShallBeTrue()
        {
            _luScriptEngine.Globals.result = 0;
            _luScriptEngine.Globals.var = "hello";
            _luScriptEngine.Execute("if var then result = 1 end");
            Assert.AreEqual(1, _luScriptEngine.Globals.result);
        }
    }
}