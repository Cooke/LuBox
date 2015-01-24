using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBox.Compiler;

namespace NuTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var nuEngine = new NuEngine();
            nuEngine.Evaluate<int>("4 * 4");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                nuEngine.Evaluate<int>("4 * 4");
            }
            Console.WriteLine("Time: {0}", stopwatch.ElapsedMilliseconds);
        }
    }
}
