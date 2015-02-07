using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
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

            //Action handler = () => Console.WriteLine("Hello");
            //test.Event += handler;
            //var eventInfo = typeof (Test).GetEvent("Event");
            //eventInfo.AddMethod.Invoke(test, new [] { handler });

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

        private class Test
        {
            public event Func<string, object> Event;

            public virtual void OnEvent()
            {
                var handler = Event;
                if (handler != null) handler("hello");
            }
        }
    }
}