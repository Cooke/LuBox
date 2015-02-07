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
            _luScriptEngine.Execute(@"
function func() 
    out = 33
end
");
        }

        [TestMethod]
        public void CallFunction()
        {
            _luScriptEngine.Execute(@"
function func() 
    out = 33
end

func()
");

            Assert.AreEqual(33, _luScriptEngine.Globals.@out);
        }

        [TestMethod]
        public void FunctionLocalScope()
        {
            _luScriptEngine.Execute(@"
function func() 
    local out = 33
end

func()
");

            Assert.IsFalse(_luScriptEngine.GlobalDictionary.ContainsKey("out"));
        }

        [TestMethod]
        public void FunctionNestedLocalScope()
        {
            _luScriptEngine.Execute(@"
function func() 
    local out = 33
end

func()

glob = out
");

            Assert.AreNotEqual(33, _luScriptEngine.Globals.glob);
        }

        [TestMethod]
        public void FunctionWithArguments()
        {
            _luScriptEngine.Execute(@"
function add(left, right) 
    out = left + right
end

add(3, 4)
");

            Assert.AreEqual(7, _luScriptEngine.Globals.@out);
        }

        [TestMethod]
        public void FunctionWithReturnValue()
        {
            _luScriptEngine.Execute(@"
function add(left, right) 
    return left + right
end

out = add(3, 9)
");

            Assert.AreEqual(12, _luScriptEngine.Globals.@out);
        }
    }
}