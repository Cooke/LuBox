using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class ConditionalInvokeGetTests
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
        public void ShallReturnValueWhenNotNullColon()
        {
            _environment.Dynamic.field1 = "hello";
            var field1 = _luScriptEngine.Evaluate<string>("field1?:ToString()", _environment);
            Assert.AreEqual("hello", field1);
        }

        [TestMethod]
        public void ShallReturnNullWhenNullColon()
        {
            var field1 = _luScriptEngine.Evaluate<string>("field1?:ToString()", _environment);
            Assert.IsNull(field1);
        }

        [TestMethod]
        public void ShallReturnValueWhenNotNullDot()
        {
            _environment.Dynamic.field1 = "hello";
            var field1 = _luScriptEngine.Evaluate<string>("field1?.ToString()", _environment);
            Assert.AreEqual("hello", field1);
        }

        [TestMethod]
        public void ShallReturnNullWhenNullDot()
        {
            var field1 = _luScriptEngine.Evaluate<string>("field1?.ToString()", _environment);
            Assert.IsNull(field1);
        }

        [TestMethod]
        public void ShallReturnNullWhenNullDotNullDot()
        {
            var field1 = _luScriptEngine.Evaluate<string>("field1?.ToString()?.Test", _environment);
            Assert.IsNull(field1);
        }

        [TestMethod]
        public void ShallReturnNullWhenNotNullDotNullDot()
        {
            _environment.Dynamic.field1 = new Tuple<string>(null);
            var field1 = _luScriptEngine.Evaluate<string>("field1?.Item1?.Test", _environment);
            Assert.IsNull(field1);
        }

        [TestMethod]
        public void ShallReturnNullWhenNotNullDotNullDotVarient()
        {
            _environment.Dynamic.field1 = new Tuple<string>(null);
            var field1 = _luScriptEngine.Evaluate<string>("field1.Item1?.Test", _environment);
            Assert.IsNull(field1);
        }

        [TestMethod]
        public void ShallReturnNotNullWhenNotNullDotNotNullDot()
        {
            _environment.Dynamic.field1 = new Tuple<string>("hello");
            var field1 = _luScriptEngine.Evaluate<string>("field1.Item1?.ToString()", _environment);
            Assert.IsNotNull(field1);
        }

        [TestMethod]
        public void ShallReturnNotNullWhenNotNullDotNotNullDotVarient()
        {
            _environment.Dynamic.field1 = new Tuple<string>("hello");
            var field1 = _luScriptEngine.Evaluate<string>("field1.Item1.ToString()", _environment);
            Assert.IsNotNull(field1);
        }

        [TestMethod]
        public void ShallReturnNotNullWhenNotNullDotNotNullDotVarient2()
        {
            _environment.Dynamic.field1 = new Tuple<string>("hello");
            var field1 = _luScriptEngine.Evaluate<string>("field1?.Item1?.ToString()", _environment);
            Assert.IsNotNull(field1);
        }

        [TestMethod]
        public void ShallOnlyCallOneTimePerSymbolReferenceTest()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter:Call():Call()", _environment);
            Assert.AreEqual(2, callCounter.Counter);
        }

        [TestMethod]
        public void ShallOnlyCallOneTimePerSymbolReferenceTestDot()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter.Call().Call()", _environment);
            Assert.AreEqual(2, callCounter.Counter);
        }

        [TestMethod]
        public void ShallOnlyCallOneTimePerSymbol()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter?:Call()?:Call()", _environment);
            Assert.AreEqual(2, callCounter.Counter);
        }

        [TestMethod]
        public void ShallOnlyCallOneTimePerSymbolDot()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter?.Call()?.Call()", _environment);
            Assert.AreEqual(2, callCounter.Counter);
        }

        [TestMethod]
        public void CallOnIndexerColon1()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter['hi']:Call()", _environment);
            Assert.AreEqual(1, callCounter.Counter);
        }

        [TestMethod]
        public void CallOnIndexerDot()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter['hi'].Call()", _environment);
            Assert.AreEqual(1, callCounter.Counter);
        }

        [TestMethod]
        public void CallOnIndexerColonQuestionMark()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter['hi']?:Call()", _environment);
            Assert.AreEqual(1, callCounter.Counter);
        }

        [TestMethod]
        public void CallOnIndexerDotQuestionMark()
        {
            var callCounter = new CallCounter();
            _environment.Dynamic.callCounter = callCounter;
            _luScriptEngine.Execute("callCounter['hi']?.Call()", _environment);
            Assert.AreEqual(1, callCounter.Counter);
        }

        private class CallCounter
        {
            private int counter;

            public CallCounter Call()
            {
                counter++;
                return this;
            }

            public int Counter
            {
                get { return counter; }
            }

            public CallCounter this[string str] => str == "hi" ? this : null;
        }
    }
}