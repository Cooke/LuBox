using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class ParsingOutputTests
    {
        [TestMethod]
        [ExpectedException(typeof(LuCompileException))]
        public void LocalTypo()
        {
            var scriptEngine = new LuScriptEngine();
            scriptEngine.Execute("locl hej = 123");
        }
    }
}
