using System;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class FunctionTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuTable _environment;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _environment = new LuTable();
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

            Assert.AreEqual(33, _environment.Dynamic.@out);
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

            Assert.IsFalse(_environment.HasField("out"));
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

            Assert.AreNotEqual(33, _environment.Dynamic.glob);
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

            Assert.AreEqual(7, _environment.Dynamic.@out);
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

            Assert.AreEqual(12, _environment.Dynamic.@out);
        }

        [TestMethod]
        public void LuFunctionShallNotBeConvertedIfNotNeeded()
        {
            _environment.Dynamic.invoke = new Func<object, object>(Invoke);
            _luScriptEngine.Execute(@"
function add() 
    return 1 + 2
end

out = invoke(add)
", _environment);

            Assert.AreEqual(3, _environment.Dynamic.@out);
        }

        [TestMethod]
        public void FunctionAsExpression()
        {
            _luScriptEngine.Execute(@"
func = function() 
    return 1 + 2
end", _environment);

            Assert.AreEqual(3, _environment.Dynamic.func());
        }

        [TestMethod]
        public void FunctionAsExpressionInLocal()
        {
            _luScriptEngine.Execute(@"
local func = function() 
    return 1 + 2
end

out = func()", _environment);

            Assert.AreEqual(3, _environment.Dynamic.@out);
        }

        [TestMethod]
        public void FunctionConvertedToFunc()
        {
            var plusOne = _luScriptEngine.Evaluate<Func<int, int>>(@"
function(input) 
    return input + 1
end", _environment);

            Assert.AreEqual(3, plusOne(2));
        }

        private static object Invoke(object func)
        {
            dynamic dfunc = func;
            
            // Some superfluouse arguments that a LuFunction should handle by ignoring
            return dfunc(123, 123);
        }
    }
}