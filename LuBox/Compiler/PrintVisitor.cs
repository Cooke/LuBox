using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Antlr4.Runtime.Tree;
using NuBox.Parser;

namespace NuBox.Compiler
{
    internal class PrintVisitor : NuBaseVisitor<Expression>
    {
        private int indent;
        public override Expression VisitNumber(NuParser.NumberContext context)
        {
            Enter();
            var temp = base.VisitNumber(context);
            Exit();
            return temp;
        }

        public override Expression VisitExp(NuParser.ExpContext context)
        {
            Enter();
            var temp = base.VisitExp(context);
            Exit();
            return temp;
        }

        public override Expression VisitChunk(NuParser.ChunkContext context)
        {
            Enter();
            var temp = base.VisitChunk(context);
            Exit();
            return temp;
        }

        public override Expression VisitStat(NuParser.StatContext context)
        {
            Enter();
            var temp = base.VisitStat(context);
            Exit();
            return temp;
        }

        public override Expression VisitChildren(IRuleNode node)
        {
            Enter();
            var temp = base.VisitChildren(node);
            Exit();
            return temp;
        }

        public override Expression VisitOperatorAddSub(NuParser.OperatorAddSubContext context)
        {
            Enter();
            var temp = base.VisitOperatorAddSub(context);
            Exit();
            return temp;
        }

        private void Enter([CallerMemberName] string caller = null)
        {
            Indent();
            indent++;
            Console.WriteLine("Enter: {0}", caller);
        }

        private void Exit([CallerMemberName] string caller = null)
        {
            indent--;
            Indent();
            Console.WriteLine("Exit: {0}", caller);
        }

        private void Indent()
        {
            for (int i = 0; i < indent; i++)
            {
                Console.Write("  ");
            }
        }
    }
}