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

        [TestMethod]
        public void ComparisonWithFloatAndDoubleInVariableShallWork()
        {
            _luScriptEngine.DefaultEnvironment.Dynamic.para = 0.5f;
            AssertEval(true, "para < 5.5");
        }

        [TestMethod]
        public void ComparisonWithTwoVariablesShallWork()
        {
            _luScriptEngine.DefaultEnvironment.Dynamic.left = 0.5f;
            _luScriptEngine.DefaultEnvironment.Dynamic.right = 5.5f;
            AssertEval(true, "left < right");
        }

        [TestMethod]
        public void EnumShallNotBeEqualToNull()
        {
            _luScriptEngine.DefaultEnvironment.Dynamic.left = MyEnum.Value1;
            AssertEval(false, "left == null");
        }

        [TestMethod]
        public void EnumShallBeNotNull()
        {
            _luScriptEngine.DefaultEnvironment.Dynamic.left = MyEnum.Value1;
            AssertEval(true, "left ~= null");
        }

        [TestMethod]
        public void EnumSameValuesShallBeEqual()
        {
            _luScriptEngine.DefaultEnvironment.Dynamic.left = MyEnum.Value1;
            _luScriptEngine.DefaultEnvironment.Dynamic.right = MyEnum.Value1;
            AssertEval(true, "left == right");
        }

        [TestMethod]
        public void EnumDifferentValuesShallNotBeEqual()
        {
            _luScriptEngine.DefaultEnvironment.Dynamic.left = MyEnum.Value1;
            _luScriptEngine.DefaultEnvironment.Dynamic.right = MyEnum.Value2;
            AssertEval(false, "left == right");
        }

        private enum MyEnum 
        {
            Value1,
            Value2
        }

        private void AssertEval<T>(T expected, string expression)
        {
            Assert.AreEqual(expected, _luScriptEngine.Evaluate<T>(expression));
        }
    }
}
