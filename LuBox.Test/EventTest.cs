using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    using System.Collections.Generic;

    [TestClass]
    public class EventTests
    {
        private LuScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        public void HandleEvent()
        {
            var test = new Test();
            _luScriptEngine.Globals.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(args) 
    out = 33
end

test.Event:Add(HandleEvent)
");
            
            Assert.IsFalse(_luScriptEngine.GlobalDictionary.ContainsKey("out"));
            test.OnEvent();
            Assert.AreEqual(33, _luScriptEngine.Globals.@out);
        }

        [TestMethod]
        public void HandleEventDifferentReturnType()
        {
            var test = new Test();
            _luScriptEngine.Globals.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(args) 
    out = 33
end

test.Event2:Add(HandleEvent)
");

            Assert.IsFalse(_luScriptEngine.GlobalDictionary.ContainsKey("out"));
            test.OnEvent2();
            Assert.AreEqual(33, _luScriptEngine.Globals.@out);
        }

        [TestMethod]
        public void HandleEventWithFewerArguments()
        {
            var test = new Test();
            _luScriptEngine.Globals.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(str) 
    msg = str
end

test.Event3:Add(HandleEvent)
");

            test.OnEvent3();
            Assert.AreEqual("string", _luScriptEngine.Globals.msg);
        }

        [TestMethod]
        public void HandleEventWithMoreArguments()
        {
            var test = new Test();
            _luScriptEngine.Globals.test = test;

            _luScriptEngine.Execute(@"
function HandleEvent(str, number, test, dic, anotherArg, additionalArg) 
    msg = anotherArg
end

test.Event3:Add(HandleEvent)
");

            test.OnEvent3();
            Assert.IsNull(_luScriptEngine.Globals.msg);
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