using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class BinaryOperatorTests
    {
        private LuScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        public void GreaterThanShallBeTrue()
        {
            AssertEval(true, "3 > 0.1");
        }

        [TestMethod]
        public void GreaterThanShallBeFalse()
        {
            AssertEval(false, "0.1 > 0.1");
        }

        [TestMethod]
        public void LessThanShallBeTrue()
        {
            AssertEval(true, "0.1 < 0.2");
        }

        [TestMethod]
        public void LessThanShallBeFalse()
        {
            AssertEval(false, "10 < -10");
        }

        [TestMethod]
        public void LessThanOrEqualShallBeFalse()
        {
            AssertEval(false, "10 <= -10");
        }

        [TestMethod]
        public void LessThanOrEqualShallBeTrue()
        {
            AssertEval(true, "10 <= 10");
        }

        [TestMethod]
        public void AndShallBeLeft()
        {
            AssertEval(false, "false and 10");
        }

        [TestMethod]
        public void AndShallBeRight()
        {
            AssertEval(10, "true and 10");
        }

        [TestMethod]
        public void OrShallBeLeft()
        {
            AssertEval("hello", "'hello' or 10");
        }

        [TestMethod]
        public void OrShallBeRight()
        {
            AssertEval(10, "false or 10");
        }

        private void AssertEval<T>(T expected, string expression)
        {
            Assert.AreEqual(expected, _luScriptEngine.Evaluate<T>(expression));
        }
    }
}
