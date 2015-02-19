using LuBox.Sandboxing;

namespace LuBox.Runtime
{
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class LuInvokeBinder : InvokeBinder
    {
        public LuInvokeBinder(CallInfo callInfo) : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !args.All(x => x.HasValue))
            {
                return Defer(target, args);
            }

            Sandboxer.ThrowIfReflectionType(target.LimitType);

            var methodInfo = target.LimitType.GetMethod("Invoke");
            var callArguments = LuInvokeHelper.TransformArguments(args, methodInfo);
            var restrictions = LuInvokeHelper.GetTypeRestrictions(target, args);

            return
                new DynamicMetaObject(
                    RuntimeHelpers.EnsureObjectResult(
                        Expression.Invoke(Expression.Convert(target.Expression, target.LimitType), callArguments)),
                    restrictions);
        }
    }
}