using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class UnaryOperatorTests
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
        public void NotTrueShallBeFalse()
        {
            AssertEval(false, "not true");
        }

        [TestMethod]
        public void NotFalseShallBeTrue()
        {
            AssertEval(true, "not false");
        }

        [TestMethod]
        public void NotTrueVariableShallBeFalse()
        {
            _environment.Variables.var = true;
            AssertEval(false, "not var");
        }

        [TestMethod]
        public void NotFalseVariableShallBeTrue()
        {
            _environment.Variables.var = false;
            AssertEval(true, "not false");
        }

        private void AssertEval<T>(T expected, string expression)
        {
            Assert.AreEqual(expected, _luScriptEngine.Evaluate<T>(expression, _environment));
        }
    }
}
