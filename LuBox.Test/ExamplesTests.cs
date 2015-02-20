using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class ExampleTests
    {
        [TestMethod]
        public void CallClrObject()
        {
            var scriptEngine = new LuScriptEngine();
            dynamic environment = scriptEngine.CreateStandardEnvironment();

            environment.myVariable = "Hello";
            scriptEngine.Execute("myVariable = myVariable:ToUpper()", environment);

            Console.WriteLine(environment.myVariable);  // Output: HELLO
        }

        [TestMethod]
        public void CallLuFunction()
        {
            var scriptEngine = new LuScriptEngine();
            dynamic environment = scriptEngine.CreateStandardEnvironment();
            
            scriptEngine.Execute(@"
                function Add(left, right)
                    return left + right
                end", environment);

            Console.WriteLine(environment.Add(1, 2));  // Output: 3
        }

        public class Foo
        {
            public event Action<string> BarFired;

            public void FireBar()
            {
                BarFired("BAR!");
            }
        }

        [TestMethod]
        public void HandleClrEvents()
        {
            var scriptEngine = new LuScriptEngine();
            dynamic environment = scriptEngine.CreateStandardEnvironment();

            var foo = new Foo();
            environment.foo = foo;
            environment.console = Console.Out;

            scriptEngine.Execute(@"
                function handleBar(text)
                    console:WriteLine(text)
                end

                foo.BarFired:Add(handleBar)", environment);

             foo.FireBar(); // Output: BAR!
        }

        [TestMethod]
        public void IterateClrEnumerable()
        {
            var scriptEngine = new LuScriptEngine();
            dynamic environment = scriptEngine.CreateStandardEnvironment();

            environment.list = new List<int> { 1, 2, 3, 4, 5 };
            scriptEngine.Execute(@"
                sum = 0
                for i in iter(list) do
                    sum = sum + i
                end
                ", environment);

            Console.WriteLine(environment.sum); // 15
        }
    }
}
