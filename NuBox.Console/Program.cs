using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuBox.Compiler;

namespace NuBox.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var nuEngine = new NuEngine();
            nuEngine.Evaluate<int>("local temp = 1 + 2");
            System.Console.ReadLine();
        }
    }
}
