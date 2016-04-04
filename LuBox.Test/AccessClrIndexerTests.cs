using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class AccessClrIndexerTests
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
        public void SetIndex()
        {
            var testDummy = new TestDummy();
            _environment.SetField("testDummy", testDummy);
            _engine.Execute("testDummy['apa'] = 'nisse'", _environment);
            Assert.AreEqual(testDummy["apa"], "nisse");
        }

        [TestMethod]
        public void GetIndex()
        {
            var testDummy = new TestDummy();
            testDummy["apan"] = "123";
            _environment.SetField("testDummy", testDummy);
            var result = _engine.Evaluate<string>("testDummy['apan']", _environment);
            Assert.AreEqual("123", result);
        }

        [TestMethod]
        public void SetIndexDynamic()
        {
            var testDummy = new TestDummyAction();
            _environment.SetField("testDummy", testDummy);
            _engine.Execute("function myFunc() end", _environment);
            _engine.Execute("testDummy['apa'] = myFunc", _environment);
        }

        [TestMethod]
        public void IndexingNilObject()
        {
            var result = _engine.Evaluate<object>("testDummy['apan']", _environment);
            Assert.IsNull(result);
        }

        public class TestDummy
        {
            private Dictionary<string, string> _dictionary = new Dictionary<string, string>();

            public string this[string key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }
        }

        public class TestDummyAction
        {
            private Dictionary<string, Action> _dictionary = new Dictionary<string, Action>();

            public Action this[string key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }
        }
    }
}
