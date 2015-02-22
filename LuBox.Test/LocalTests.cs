using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class LocalTests
    {
        [TestMethod]
        public void SetLocalBool()
        {
            var scriptEngine = new LuScriptEngine();
            scriptEngine.Execute("local var = true; var = false");
        }
    }
}
