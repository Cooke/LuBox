using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace NuBox.Parser
{
    public partial class NuParser
    { 
        public partial class NumberContext
        {
            public int AsInt()
            {
                return int.Parse(this.INT().Symbol.Text);
            }
        }
    }
}
