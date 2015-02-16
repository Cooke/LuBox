using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    using System.Collections.Generic;

    [TestClass]
    public class EventTests
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
        public void HandleEvent()
        {
            var test = new Test();
            _environment.Variables.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(args) 
    out = 33
end

test.Event:Add(HandleEvent)
", _environment);

            Assert.IsFalse(_environment.Dictionary.ContainsKey("out"));
            test.OnEvent();
            Assert.AreEqual(33, _environment.Variables.@out);
        }

        [TestMethod]
        public void HandleEventDifferentReturnType()
        {
            var test = new Test();
            _environment.Variables.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(args) 
    out = 33
end

test.Event2:Add(HandleEvent)
", _environment);

            Assert.IsFalse(_environment.Dictionary.ContainsKey("out"));
            test.OnEvent2();
            Assert.AreEqual(33, _environment.Variables.@out);
        }

        [TestMethod]
        public void HandleEventWithFewerArguments()
        {
            var test = new Test();
            _environment.Variables.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(str) 
    msg = str
end

test.Event3:Add(HandleEvent)
", _environment);

            test.OnEvent3();
            Assert.AreEqual("string", _environment.Variables.msg);
        }

        [TestMethod]
        public void HandleEventWithMoreArguments()
        {
            var test = new Test();
            _environment.Variables.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(str, number, test, dic, anotherArg, additionalArg) 
    msg = anotherArg
end

test.Event3:Add(HandleEvent)
", _environment);

            test.OnEvent3();
            Assert.IsNull(_environment.Variables.msg);
        }

        [TestMethod]
        public void ShallRemoveEventHandler()
        {
            var test = new Test();
            _environment.Variables.test = test;

            _luScriptEngine.Execute(@"
                function HandleEvent() 
                    called = 1
                end

                test.Event:Add(HandleEvent)
                test.Event:Remove(HandleEvent)
                ", _environment);

            test.OnEvent();
            Assert.IsFalse(_environment.Dictionary.ContainsKey("called"));
        }

        private class Test
        {
            public event Func<string, object> Event;

            public event Action<string> Event2;

            public event Action<string, int, Test, Dictionary<string, string>> Event3;

            public virtual void OnEvent3()
            {
                Action<string, int, Test, Dictionary<string, string>> handler = Event3;
                if (handler != null)
                {
                    handler("string", 123, this, new Dictionary<string, string>());
                }
            }

            public virtual void OnEvent2()
            {
                Action<string> handler = Event2;
                if (handler != null)
                {
                    handler("hello2");
                }
            }

            public virtual void OnEvent()
            {
                var handler = Event;
                if (handler != null) handler("hello");
            }
        }
    }
}