using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class LocalVariableTests
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
        public void SetLocalVariable()
        {
            _luScriptEngine.Execute(@"
local result= 3 * 4
global = result * 2
", _environment);
            Assert.IsFalse(_environment.HasField("result"));
            Assert.AreEqual(24, _environment.Dynamic.global);
        }
    }
}