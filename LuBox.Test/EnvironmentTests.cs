using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBox.Compiler;

namespace NuTest
{
    [TestClass]
    public class EnvironmentTests
    {
        private NuEngine _nuEngine;

        [TestInitialize]
        public void Initialize()
        {
            _nuEngine = new NuEngine();
        }
    }
}