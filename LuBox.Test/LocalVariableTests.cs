using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class LocalVariableTests
    {
        private LuScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        public void SetLocalVariable()
        {
            _luScriptEngine.Execute(@"
local result= 3 * 4
global = result * 2
");
            Assert.IsFalse(_luScriptEngine.GlobalDictionary.ContainsKey("result"));
            Assert.AreEqual(24, _luScriptEngine.Globals.global);
        }
    }
}