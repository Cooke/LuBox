using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class GenericMethodsTests
    {
        [TestMethod]
        public void ArgIsGeneric()
        {
            var scriptEngine = new LuScriptEngine(typeof(Enumerable));
            scriptEngine.DefaultEnvironment.Dynamic.array = new[] {10, 20, 30};
            var i = scriptEngine.Evaluate<int>("array:FirstOrDefault(function(x) return x > 15 end)");
            Assert.AreEqual(20, i);
        }

        [TestMethod]
        public void ArgIsGenericParameter()
        {
            var scriptEngine = new LuScriptEngine();
            scriptEngine.DefaultEnvironment.Dynamic.test = new Test();
            var ret = scriptEngine.Evaluate("test:TestMethod(123)");
            Assert.AreEqual(123, ret);
        }

        [TestMethod]
        public void ArgIsGenericInGeneric()
        {
            var scriptEngine = new LuScriptEngine();
            scriptEngine.DefaultEnvironment.Dynamic.test = new Test();
            scriptEngine.DefaultEnvironment.Dynamic.matchingArg = new[] { new[] { 123 } };
            var ret = scriptEngine.Evaluate("test:TestGenericInGeneric(matchingArg)");
            Assert.AreEqual(scriptEngine.DefaultEnvironment.Dynamic.matchingArg, ret);
        }

        public class Test 
        {
            public TArg TestMethod<TArg>(TArg arg)
            {
                return arg;
            }

            public IEnumerable<IEnumerable<TArg>> TestGenericInGeneric<TArg>(IEnumerable<IEnumerable<TArg>> arg)
            {
                return arg;
            }
        }
    }
}
