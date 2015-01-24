using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBox.Compiler;

namespace NuTest
{
    [TestClass]
    public class ArithmeticTests
    {
        private NuEngine _nuEngine;

        [TestInitialize]
        public void Initialize()
        {
            _nuEngine = new NuEngine();
        }

        [TestMethod]
        public void AddOne()
        {
            AssertEval(3, "1 + 2");
        }

        [TestMethod]
        public void AddOneNegative()
        {
            AssertEval(-1, "1 + -2");
        }

        [TestMethod]
        public void AddOneToNegative()
        {
            AssertEval(1, "-1 + 2");
        }

        [TestMethod]
        public void AddTwo()
        {
            AssertEval(6, "1 + 2 + 3");            
        }

        [TestMethod]
        public void AddAndSubtract()
        {
            AssertEval(0, "1 + 2 - 3");
        }

        [TestMethod]
        public void SubtractOne()
        {
            AssertEval(1, "4 - 3");
        }

        [TestMethod]
        public void SubtractTwo()
        {
            AssertEval(-9, "4 - 3 - 10");
        }

        [TestMethod]
        public void AddOneInParanthesis()
        {
            AssertEval(5, "(2 + 3)");
        }

        [TestMethod]
        public void ParanthesRight()
        {
            AssertEval(1, "5 - (2 + 2)");
        }

        [TestMethod]
        public void ParanthesLeft()
        {
            AssertEval(5, "(5 - 2) + 2");
        }

        [TestMethod]
        public void ParanthesOverThree()
        {
            AssertEval(2, "(5 - 5 + 2)");
        }

        [TestMethod]
        public void NestedParanthesTwo()
        {
            AssertEval(-2, "(5 - (5 + 2))");
        }

        [TestMethod]
        public void NestedParanthesThree()
        {
            AssertEval(-2, "(5 - (5 + (2)))");
        }

        [TestMethod]
        public void MultiplyOne()
        {
            AssertEval(35, "5 * 7");
        }

        [TestMethod]
        public void MultiplyTwo()
        {
            AssertEval(70, "5 * 7 * 2");
        }

        [TestMethod]
        public void MultiplyWithParenthesis()
        {
            AssertEval(70, "5 * (7 * 2)");
        }

        [TestMethod]
        public void MultiplyWithNegative()
        {
            AssertEval(-10, "-5 * 2");
        }

        [TestMethod]
        public void AddFloat()
        {
            AssertEval(0.75, "0.5 + 0.25");
        }

        [TestMethod]
        public void AddFloatToInt()
        {
            AssertEval(3.5, "0.5 + 3");
        }

        [TestMethod]
        public void MultiplayFloatWithInt()
        {
            AssertEval(110.0, "0.25 * 440");
        }

        [TestMethod]
        public void MultiplayTwoFloat()
        {
            AssertEval(0.0625, "0.25 * 0.25");
        }

        [TestMethod]
        public void Divide()
        {
            AssertEval(0.25, "4 / 16");
        }

        [TestMethod]
        public void DivideWithFloat()
        {
            AssertEval(16, "4 / 0.25");
        }

        [TestMethod]
        public void TestObjectResult()
        {
            object o = _nuEngine.Evaluate("3 + 3");
            Assert.AreEqual(6, o);
        }

        private void AssertEval<T>(T expected, string expression)
        {
            Assert.AreEqual(expected, _nuEngine.Evaluate<T>(expression));
        }
    }
}
