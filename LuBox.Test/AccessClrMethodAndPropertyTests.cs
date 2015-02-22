using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class AccessClrMethodAndPropertyTests
    {
        private LuScriptEngine _engine;
        private LuTable _environment;

        [TestInitialize]
        public void Initialize()
        {
            _engine = new LuScriptEngine();
            _environment = _engine.CreateStandardEnvironment();
        }

        [TestMethod]
        public void CallMember()
        {
            var getter = new CallReceiver();
            _environment.Dynamic.callReceiver = getter;
            _engine.Execute("callReceiver:Call()", _environment);
            Assert.AreEqual("Call", getter.Called);
        }

        [TestMethod]
        public void CallMemberVoid()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            _engine.Execute("callReceiver:CallVoid()", _environment);
            Assert.AreEqual("CallVoid", getter.Called);
        }

        [TestMethod]
        public void CallMemberVoidString()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            _engine.Execute("callReceiver:CallVoid(\"Hello\")", _environment);
            Assert.AreEqual("CallVoid(messages)", getter.Called);
            Assert.AreEqual(1, getter.Counter);
        }

        [TestMethod]
        public void CallMemberParams()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            _engine.Execute("callReceiver:CallVoidParams(\"Hello\", 123, 456, 0.3)", _environment);
            Assert.AreEqual("CallVoidParams", getter.Called);
        }

        [TestMethod]
        public void GetProperty()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            var propValue = _engine.Evaluate<string>("callReceiver.Property", _environment);
            Assert.AreEqual("value", propValue);
        }

        [TestMethod]
        public void GetPropertyThenCall()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            var propValue = _engine.Evaluate<string>("callReceiver.Property:ToString()", _environment);
            Assert.AreEqual("value", propValue);
        }

        [TestMethod]
        public void UseGlobalVariable()
        {
            _environment.SetField("factor", 3);
            var result = _engine.Evaluate<int>("factor * 3", _environment);
            Assert.AreEqual(9, result);
        }

        [TestMethod]
        public void ChainProperties()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            var propValue = _engine.Evaluate<CallReceiver>("callReceiver.Self.Self.Self", _environment);
            Assert.AreSame(getter, propValue);
        }

        [TestMethod]
        public void ChainMethods()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            var propValue = _engine.Evaluate<CallReceiver>("callReceiver:GetSelf():GetSelf():GetSelf()", _environment);
            Assert.AreSame(getter, propValue);
            Assert.AreEqual(3, getter.Counter);
        }

        [TestMethod]
        public void ChainMethodAndProperties()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            var propValue = _engine.Evaluate<String>("callReceiver:GetSelf().Property:ToUpper()", _environment);
            Assert.AreEqual(getter.Property.ToUpper(), propValue);
        }

        [TestMethod]
        public void CallMethodWithDotAnnotation()
        {
            var getter = new CallReceiver();
            _environment.SetField("callReceiver", getter);
            var self = _engine.Evaluate<CallReceiver>("callReceiver.GetSelf()", _environment);
            Assert.AreEqual(getter, self);
        }

        public class CallReceiver
        {
            private string _called;
            private int _counter;

            public string Called
            {
                get { return _called; }
            }

            public int Counter
            {
                get { return _counter; }
            }

            public string Property
            {
                get { return "value"; }
            }

            public CallReceiver Self
            {
                get { return this; }
            }

            public CallReceiver GetSelf()
            {
                _counter++;
                return this; 
            }

            public object Call()
            {
                _counter++;
                _called = "Call";
                return _called;
            }

            public void CallVoid()
            {
                _counter++;
                _called = "CallVoid";
            }

            public void CallVoid(string messages)
            {
                _counter++;
                _called = "CallVoid(messages)";
            }

            public void CallVoidParams(string messages, params object[] args)
            {
                _counter++;
                _called = "CallVoidParams";
            }
        }
    }
}
