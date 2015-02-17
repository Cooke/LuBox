using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class ExampleTests
    {
        [TestMethod]
        public void CallMember()
        {
            var scriptEngine = new LuScriptEngine();
            var environment = new LuTable();
            environment.Dynamic.myVariable = "Hello";
            scriptEngine.Execute("myVariable = myVariable:ToUpper()", environment);
            Console.WriteLine(environment.Dynamic.myVariable);  // Output: HELLO
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
        public void RegisteringToEvents()
        {
            var scriptEngine = new LuScriptEngine();
            var environment = new LuTable();
            Foo foo = new Foo();
            environment.Dynamic.foo = foo;
            environment.Dynamic.console = Console.Out;
            scriptEngine.Execute(
                @"
                function handleBar(text)
                    console:WriteLine(text)
                end

                foo.BarFired:Add(handleBar)", environment);
             foo.FireBar(); // Output: BAR!
        }
    }
}
