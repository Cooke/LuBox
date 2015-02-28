using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LuBox.PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new LuScriptEngine();
            var environment = engine.CreateStandardEnvironment();
            environment.SetField("callReceivers", new List<CallReceiver> { new CallReceiver(), new CallReceiver() });

            var script1 = engine.CompileBind("for callReceiver in iter(callReceivers) do for i = 1, 1000000, 1 do callReceiver.GetSelf() end end", environment);
            var script2 = engine.CompileBind("for callReceiver in iter(callReceivers) do for i = 1, 1000000, 1 do callReceiver:GetSelf() end end", environment);

            // Warm up
            script1();
            script2();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            script1();
            stopwatch.Stop();
            Console.WriteLine("1 - Time: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch = new Stopwatch();
            stopwatch.Start();
            script2();
            stopwatch.Stop();
            Console.WriteLine("2 - Time: {0}", stopwatch.ElapsedMilliseconds);

            Console.ReadLine();
        }

        public class CallReceiver
        {
            private string _called;
            private int _counter;

            public string Called
            {
                get { return _called; }
            }

            public int Counter
            {
                get { return _counter; }
            }

            public string Property
            {
                get { return "value"; }
            }

            public CallReceiver Self
            {
                get { return this; }
            }

            public CallReceiver GetSelf()
            {
                _counter++;
                return this;
            }

            public object Call()
            {
                _counter++;
                _called = "Call";
                return _called;
            }

            public void CallVoid()
            {
                _counter++;
                _called = "CallVoid";
            }

            public void CallVoid(string messages)
            {
                _counter++;
                _called = "CallVoid(messages)";
            }

            public void CallVoidParams(string messages, params object[] args)
            {
                _counter++;
                _called = "CallVoidParams";
            }
        }
    }
}
