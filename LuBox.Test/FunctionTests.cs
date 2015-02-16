using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class FunctionTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuEnvironment _environment;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _environment = new LuEnvironment();
        }

        [TestMethod]
        public void DefineFunction()
        {
            _luScriptEngine.Execute(@"
function func() 
    out = 33
end
", _environment);
        }

        [TestMethod]
        public void CallFunction()
        {
            _luScriptEngine.Execute(@"
function func() 
    out = 33
end

func()
", _environment);

            Assert.AreEqual(33, _environment.Variables.@out);
        }

        [TestMethod]
        public void FunctionLocalScope()
        {
            _luScriptEngine.Execute(@"
function func() 
    local out = 33
end

func()
", _environment);

            Assert.IsFalse(_environment.Dictionary.ContainsKey("out"));
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
", _environment);

            Assert.AreNotEqual(33, _environment.Variables.glob);
        }

        [TestMethod]
        public void FunctionWithArguments()
        {
            _luScriptEngine.Execute(@"
function add(left, right) 
    out = left + right
end

add(3, 4)
", _environment);

            Assert.AreEqual(7, _environment.Variables.@out);
        }

        [TestMethod]
        public void FunctionWithReturnValue()
        {
            _luScriptEngine.Execute(@"
function add(left, right) 
    return left + right
end

out = add(3, 9)
", _environment);

            Assert.AreEqual(12, _environment.Variables.@out);
        }
    }
}