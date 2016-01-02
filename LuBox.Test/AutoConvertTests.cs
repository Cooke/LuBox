using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class AutoConvertTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuTable _env;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _env = new LuTable();
            _env.Dynamic.TestClass = new MyClass();
        }

        [TestMethod]
        public void TestObjectResult()
        {
            _luScriptEngine.Execute("TestClass.IntProp = 33 / 10", _env);
            Assert.AreEqual(3, _env.Dynamic.TestClass.IntProp);
        }

        private class MyClass
        {
            public int IntProp { get; set; }
        }
    }
}
