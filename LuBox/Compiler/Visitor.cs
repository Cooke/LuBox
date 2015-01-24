using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LuBox.Parser;
using LuBox.Runtime;

namespace LuBox.Compiler
{
    internal class Visitor : NuBaseVisitor<Expression>
    {
        private readonly ParameterExpression _environmentVariable;

        public Visitor(ParameterExpression environmentVariable)
        {
            _environmentVariable = environmentVariable;
        }

        public override Expression VisitNumber(NuParser.NumberContext context)
        {
            if (context.INT() != null)
            {
                return Expression.Constant(int.Parse(context.INT().GetText()));
            }
            else if (context.FLOAT() != null)
            {
                return Expression.Constant(double.Parse(context.FLOAT().Symbol.Text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
            else 
            {
                throw new NotSupportedException("Number type is not supported");
            }
        }

        public override Expression VisitString(NuParser.StringContext context)
        {
            return Expression.Constant(context.GetText());
        }

        public override Expression VisitPrefixexp(NuParser.PrefixexpContext context)
        {
            Expression leftExp;
            if (context.varOrExp().exp() != null)
            {
                leftExp = VisitExp(context.varOrExp().exp());
            }
            else 
            {
                leftExp = VisitVar(context.varOrExp().var());
            }
            
            return ProcessNameAndArgs(context.nameAndArgs(), leftExp);
        }

        private Expression ProcessNameAndArgs(IEnumerable<NuParser.NameAndArgsContext> nameAndArgsList, Expression leftExp)
        {
            foreach (var nameAndArgsContext in nameAndArgsList)
            {
                string memberName = nameAndArgsContext.NAME().GetText();
                IEnumerable<Expression> args = new[] {leftExp};
                if (nameAndArgsContext.args().explist() != null)
                {
                    args = args.Concat(nameAndArgsContext.args().explist().exp().Select(VisitExp));
                }

                leftExp = Expression.Dynamic(
                    new NuInvokeMemberBinder(memberName, false, new CallInfo(nameAndArgsContext.args().ChildCount)),
                    typeof (object), args);
            }

            return leftExp;
        }

        public override Expression VisitVar(NuParser.VarContext context)
        {
            Expression methodCallExpression = Expression.Call(_environmentVariable, typeof(Scope).GetMethod("Get"), Expression.Constant(context.NAME().GetText()));
            foreach (var suffix in context.varSuffix())
            {
                // TODO support more than just properties (methods, events, ...)
                methodCallExpression =
                    Expression.Dynamic(new NuPropertyMemberBinder(suffix.NAME().GetText(), false, new CallInfo(0)),
                        typeof(object), methodCallExpression);
            }

            return methodCallExpression;
        }

        public override Expression VisitExp(NuParser.ExpContext context)
        {
            if (context.ChildCount == 3)
            {
                return VisitBinaryOperatorExp(context);
            }
            else if (context.ChildCount == 2)
            {
                return VisitUnaryOperatorExp(context);
            }
            else
            {
                return VisitSingleChildExp(context);
            }
        }

        private Expression VisitSingleChildExp(NuParser.ExpContext context)
        {
            var parseTree = context.GetChild(0);
            var ruleIndex = parseTree.GetRuleIndex();
            switch (ruleIndex)
            {
                case NuParser.RULE_prefixexp:
                    return VisitPrefixexp(context.prefixexp());
                case NuParser.RULE_number:
                    return VisitNumber(context.number());
                case NuParser.RULE_string:
                    return VisitString(context.@string());
                default:
                    throw new ParseCanceledException("Invalid expression");
            }
        }

        private Expression VisitUnaryOperatorExp(NuParser.ExpContext context)
        {
            var unaryOperator = context.operatorUnary();
            var exp = context.exp(0);
            switch (unaryOperator.GetText())
            {
                case "-":
                    return Expression.Negate(VisitExp(exp));
                default:
                    throw new ParseCanceledException("Invalid expression");
            }
        }

        private Expression VisitBinaryOperatorExp(NuParser.ExpContext context)
        {
            Expression left = VisitExp(context.exp(0));
            IParseTree oper = context.GetChild(1);
            Expression right = VisitExp(context.exp(1));
            
            var ruleIndex = oper.GetRuleIndex();
            switch (ruleIndex)
            {
                case NuParser.RULE_operatorMulDivMod:
                    return
                        Expression.Dynamic(
                            new NuBinaryOperationBinder(oper.GetText() == "*"
                                ? ExpressionType.Multiply
                                : ExpressionType.Divide), typeof(object), left, right);
                case NuParser.RULE_operatorAddSub:
                    return
                        Expression.Dynamic(
                            new NuBinaryOperationBinder(oper.GetText() == "+"
                                ? ExpressionType.Add
                                : ExpressionType.Subtract), typeof (object),
                            left, right);
                default:
                    throw new ParseCanceledException("Invalid operator");
            }
        }


        public override Expression VisitStat(NuParser.StatContext context)
        {
            return Expression.Block(VisitFunctioncall(context.functioncall()));
        }

        public override Expression VisitFunctioncall(NuParser.FunctioncallContext context)
        {
            Expression leftExp;
            if (context.varOrExp().exp() != null)
            {
                leftExp = VisitExp(context.varOrExp().exp());
            }
            else
            {
                leftExp = VisitVar(context.varOrExp().var());
            }

            return ProcessNameAndArgs(context.nameAndArgs(), leftExp);
        }


        public override Expression VisitBlock(NuParser.BlockContext context)
        {
            return Expression.Block(context.children.Select(Visit));
        }

        public override Expression VisitChunk(NuParser.ChunkContext context)
        {
            var expressions = new List<Expression>();
            foreach (var child in context.children)
            {
                var expression = Visit(child);
                if (expression != null)
                {
                    expressions.Add(expression);
                }
            }

            return Expression.Block(expressions);
        }
    }
}