using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LuBox.Runtime
{
    internal class LuMethodWrapper : IDynamicMetaObjectProvider
    {
        private readonly MethodInfo[] _methodInfos;
        private readonly object _target;

        public LuMethodWrapper(MethodInfo[] methodInfos, object target)
        {
            _methodInfos = methodInfos;
            _target = target;
        }

        public MethodInfo[] MethodInfos { get { return _methodInfos; } }

        public object Target
        {
            get { return _target; }
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuMethodWrapperMetaObject(parameter, _methodInfos, _target.GetType(), this);
        }

        internal class LuMethodWrapperMetaObject : DynamicMetaObject
        {
            private readonly MethodInfo[] _methodInfos;
            private readonly Type _targetType;

            public LuMethodWrapperMetaObject(Expression expression, MethodInfo[] methodInfos, Type targetType, object instance)
                : base(expression, BindingRestrictions.Empty, instance)
            {
                _methodInfos = methodInfos;
                _targetType = targetType;
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                var orderedSignatures = LuInvokeHelper.OrderSignatureMatches(args, _methodInfos);
                var methodInfo = orderedSignatures.First();

                var callArguments = LuInvokeHelper.TransformArguments(args, methodInfo);
                var targetExpression = Expression.Property(Expression.Convert(Expression, typeof (LuMethodWrapper)), "Target");

                var argRestrictrions = args.Select(x => BindingRestrictions.GetTypeRestriction(x.Expression, x.LimitType));
                var targetRestriction = BindingRestrictions.GetTypeRestriction(targetExpression, _targetType);
                var argRestriction = LuInvokeHelper.CombineRestrictions(argRestrictrions.ToArray());
                var restriction = LuInvokeHelper.CombineRestrictions(argRestriction, targetRestriction);

                return
                    new DynamicMetaObject(
                        RuntimeHelpers.EnsureObjectResult(
                            Expression.Call(Expression.Convert(targetExpression, _targetType), methodInfo, callArguments)),
                        restriction);
            }
        }
    }
}