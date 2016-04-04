using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class LambdaTests
    {
        [TestMethod]
        public void Simple()
        {
            var scriptEngine = new LuScriptEngine();
            var func = scriptEngine.Evaluate<Func<object, object>>("x => x");
            Assert.AreEqual(20, func(20));
        }

        [TestMethod]
        public void LambdaInLinq()
        {
            var scriptEngine = new LuScriptEngine(typeof(Enumerable));
            scriptEngine.DefaultEnvironment.Dynamic.array = new[] { 10, 20, 30 };
            var i = scriptEngine.Evaluate<int>("array:FirstOrDefault(x => x > 15)");
            Assert.AreEqual(20, i);
        }
    }
}
