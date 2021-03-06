﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LuBox.Runtime;

namespace LuBox.InternalTypes
{
    internal class DelegateWrapper : IDynamicMetaObjectProvider
    {
        private readonly Delegate[] _overloads;
        private readonly MethodInfo[] _methodInfos;

        public DelegateWrapper(Delegate[] overloads)
        {
            _overloads = overloads;
            _methodInfos = overloads.Select(x => x.Method).ToArray();
        }

        public Delegate[] Overloads
        {
            get { return _overloads; }
        }

        public MethodInfo[] MethodInfos
        {
            get { return _methodInfos; }
        }

        public bool IsMatching(IEnumerable<MethodInfo> methodInfos)
        {
            return methodInfos.SequenceEqual(_overloads.Select(x => x.Method));
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuOverloadWrapperMetaObject(parameter, this);
        }

        public class LuOverloadWrapperMetaObject : DynamicMetaObject
        {
            private readonly DelegateWrapper _value;

            public LuOverloadWrapperMetaObject(Expression parameter, DelegateWrapper value)
                : base(parameter, BindingRestrictions.Empty, value)
            {
                _value = value;
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                var orderedSignatures = SignatureHelper.OrderSignatureMatches(args, _value.MethodInfos);
                var methodInfo = orderedSignatures.First();
                var index = Array.IndexOf(_value.MethodInfos, methodInfo);

                var callArguments = SignatureHelper.TransformArguments(args, methodInfo);

                var delegateExpression = Expression.ArrayIndex(
                    Expression.MakeMemberAccess(Expression.Convert(Expression, typeof (DelegateWrapper)),
                        typeof (DelegateWrapper).GetProperty("Overloads")), Expression.Constant(index));

                var argRestrictrions = args.Select(x => BindingRestrictions.GetTypeRestriction(x.Expression, x.LimitType));
                var methodInfosrestriction = BindingRestrictions.GetExpressionRestriction(
                    Expression.Call(Expression.Convert(Expression, typeof (DelegateWrapper)),
                        typeof (DelegateWrapper).GetMethod("IsMatching"),
                        Expression.Constant(_value.MethodInfos)));
                var argRestriction = RestrictionHelper.CombineRestrictions(argRestrictrions.ToArray());
                var restriction = RestrictionHelper.CombineRestrictions(argRestriction, methodInfosrestriction);

                return
                    new DynamicMetaObject(
                        ResultHelper.EnsureObjectResult(
                            Expression.Invoke(Expression.Convert(delegateExpression, _value.Overloads[index].GetType()), callArguments)),
                        restriction);
            }
        }
    }
}