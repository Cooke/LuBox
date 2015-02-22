using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class ClosureTests
    {
        private LuScriptEngine _engine;
        private dynamic _environment;

        [TestInitialize]
        public void Initialize()
        {
            _engine = new LuScriptEngine();
            _environment = new LuTable();
        }

        [TestMethod]
        public void DefineFunction()
        {
            _engine.Execute(@"
local var = 1
function func() 
    var = var +  1;
    return var;
end
", _environment);

            Assert.AreEqual(2, _environment.func());
            Assert.AreEqual(3, _environment.func());
        }
    }
}