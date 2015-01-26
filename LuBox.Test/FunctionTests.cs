using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class FunctionTests
    {
        private LuScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
        }

        [TestMethod]
        public void DefineFunction()
        {
            _luScriptEngine.Execute("function() ");
        }
    }
}