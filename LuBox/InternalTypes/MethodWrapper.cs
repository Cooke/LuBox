using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LuBox.Runtime
{
    internal class MethodWrapper : IDynamicMetaObjectProvider
    {
        private readonly MethodInfo[] _methodInfos;
        private readonly object _target;

        public MethodWrapper(MethodInfo[] methodInfos, object target)
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
                var orderedSignatures = SignatureHelper.OrderSignatureMatches(args, _methodInfos);
                var methodInfo = orderedSignatures.First();

                var callArguments = SignatureHelper.TransformArguments(args, methodInfo);
                var targetExpression = Expression.Property(Expression.Convert(Expression, typeof (MethodWrapper)), "Target");

                var argRestrictrions = args.Select(x => BindingRestrictions.GetTypeRestriction(x.Expression, x.LimitType));
                var targetRestriction = BindingRestrictions.GetTypeRestriction(targetExpression, _targetType);
                var argRestriction = RestrictionHelper.CombineRestrictions(argRestrictrions.ToArray());
                var restriction = RestrictionHelper.CombineRestrictions(argRestriction, targetRestriction);

                return
                    new DynamicMetaObject(
                        ResultHelper.EnsureObjectResult(
                            Expression.Call(Expression.Convert(targetExpression, _targetType), methodInfo, callArguments)),
                        restriction);
            }
        }
    }
}