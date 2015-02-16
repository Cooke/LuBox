using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class ConditionalTests
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
        public void IfTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if true then result = 1 end", _environment);
            Assert.AreEqual(1, _environment.Variables.result);
        }

        [TestMethod]
        public void IfNegativeTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if false then result = 1 end", _environment);
            Assert.AreEqual(0, _environment.Variables.result);
        }

        [TestMethod]
        public void IfThenElseIfTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if true then result = 1 elseif true result = 2 end", _environment);
            Assert.AreEqual(1, _environment.Variables.result);
        }

        [TestMethod]
        public void IfThenElseIfNegativeTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if false then result = 1 elseif true result = 2 end", _environment);
            Assert.AreEqual(2, _environment.Variables.result);
        }

        [TestMethod]
        public void IfThenElseIfNegativeNegativeTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if false then result = 1 elseif false result = 2 end", _environment);
            Assert.AreEqual(0, _environment.Variables.result);
        }

        [TestMethod]
        public void IfThenTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if true then result = 1 else result = 2 end", _environment);
            Assert.AreEqual(1, _environment.Variables.result);
        }

        [TestMethod]
        public void IfThenNegativeTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if false then result = 1 else result = 2 end", _environment);
            Assert.AreEqual(2, _environment.Variables.result);
        }

        [TestMethod]
        public void IfThenElseIfElseTest()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if false then result = 1 elseif false result = 2 else result = 3 end", _environment);
            Assert.AreEqual(3, _environment.Variables.result);
        }

        [TestMethod]
        public void NumberShallBeTrue()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if 1 then result = 1 end", _environment);
            Assert.AreEqual(1, _environment.Variables.result);
        }

        [TestMethod]
        public void NumberZeroShallBeTrueAccordingToLuaManual()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if 0 then result = 1 end", _environment);
            Assert.AreEqual(1, _environment.Variables.result);
        }

        [TestMethod]
        public void NullShallBeFalse()
        {
            _environment.Variables.result = 0;
            _luScriptEngine.Execute("if undefinedVar then result = 1 end", _environment);
            Assert.AreEqual(0, _environment.Variables.result);
        }

        [TestMethod]
        public void TrueVariableShallBeTrue()
        {
            _environment.Variables.result = 0;
            _environment.Variables.var = true;
            _luScriptEngine.Execute("if var then result = 1 end", _environment);
            Assert.AreEqual(1, _environment.Variables.result);
        }

        [TestMethod]
        public void FlaseVariableShallBeFalse()
        {
            _environment.Variables.result = 0;
            _environment.Variables.var = false;
            _luScriptEngine.Execute("if var then result = 1 end", _environment);
            Assert.AreEqual(0, _environment.Variables.result);
        }

        [TestMethod]
        public void StringVariableShallBeTrue()
        {
            _environment.Variables.result = 0;
            _environment.Variables.var = "hello";
            _luScriptEngine.Execute("if var then result = 1 end", _environment);
            Assert.AreEqual(1, _environment.Variables.result);
        }
    }
}