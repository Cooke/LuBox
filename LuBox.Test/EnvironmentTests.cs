using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
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