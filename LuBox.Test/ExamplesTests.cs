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
            scriptEngine.Globals.myVariable = "Hello";
            scriptEngine.Execute("myVariable = myVariable:ToUpper()");
            Console.WriteLine(scriptEngine.Globals.myVariable);  // Output: HELLO
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
            Foo foo = new Foo();
            scriptEngine.Globals.foo = foo;
            scriptEngine.Globals.console = Console.Out;
            scriptEngine.Execute(
                @"
                function handleBar(text)
                    console:WriteLine(text)
                end

                foo.BarFired:Add(handleBar)");
             foo.FireBar(); // Output: BAR!
        }
    }
}
