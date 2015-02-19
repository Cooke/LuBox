using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class MathLibraryTests
    {
        private LuScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        public void RandomWithoutArgumentsShallBeBetweenZeroAndOne()
        {
            var result = _luScriptEngine.Evaluate<double>("math.random()");
            Assert.IsTrue(result >= 0 && result < 1);
        }

        [TestMethod]
        public void RandomWithoutArgumentsShallReturnDouble()
        {
            var result = _luScriptEngine.Evaluate("math.random()");
            Assert.IsInstanceOfType(result, typeof(double));
        }

        [TestMethod]
        public void RandomWithOneArgumentShallReturnUpToArgument()
        {
            var result = _luScriptEngine.Evaluate<double>("math.random(100)");
            Assert.IsTrue(result > 0 && result <= 100);
        }

        [TestMethod]
        public void RandomWithOneArgumentShallReturnInteger()
        {
            var result = _luScriptEngine.Evaluate("math.random(100)");
            Assert.IsInstanceOfType(result, typeof(int));
        }

        [TestMethod]
        public void RandomWithTwoArgumentShallReturnBetweenArguments()
        {
            var result = _luScriptEngine.Evaluate<double>("math.random(100, 200)");
            Assert.IsTrue(result >= 100 && result <= 200);
        }

        [TestMethod]
        public void RandomWithTwoArgumentShallReturnInteger()
        {
            var result = _luScriptEngine.Evaluate("math.random(100, 200)");
            Assert.IsInstanceOfType(result, typeof(int));
        }
    }
}
