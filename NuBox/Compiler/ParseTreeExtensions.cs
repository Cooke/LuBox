using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace NuBox.Compiler
{
    internal static class ParseTreeExtensions
    {
        public static int GetRuleIndex(this IParseTree tree)
        {
            return ((ParserRuleContext)tree).RuleIndex;
        }
    }
}