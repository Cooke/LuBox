using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class FunctionTests
    {
        private NuEngine _nuEngine;

        [TestInitialize]
        public void Initialize()
        {
            _nuEngine = new NuEngine();
        }

        [TestMethod]
        public void DefineFunction()
        {
            _nuEngine.Execute("function() ");
        }
    }
}