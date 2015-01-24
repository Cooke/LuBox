using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBox.Compiler;

namespace NuTest
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