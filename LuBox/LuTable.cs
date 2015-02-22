﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LuBox.Compiler;
using LuBox.Runtime;

namespace LuBox
{
    public class LuTable : IDynamicMetaObjectProvider
    {
        public static MethodInfo SetMethodInfo = typeof(LuTable).GetMethod("SetField");
        public static MethodInfo GetMethodInfo = typeof(LuTable).GetMethod("GetField");

        private readonly IDictionary<object, object> _fields;
        private readonly DynamicDictionaryWrapper _dynamic;

        public LuTable() : this(Enumerable.Empty<KeyValuePair<object, object>>())
        {
        }

        public LuTable(IEnumerable<KeyValuePair<object, object>> initials)
        {
            _fields = initials.ToDictionary(x => x.Key, x => x.Value);
            _dynamic = new DynamicDictionaryWrapper(_fields);
        }

        public dynamic Dynamic
        {
            get { return this; }
        }

        public bool HasField(object key)
        {
            return _fields.ContainsKey(key);
        }

        public object SetField(object key, object value)
        {
            _fields[key] = value;
            return value;
        }

        public object GetField(object key)
        {
            return _fields.ContainsKey(key) ? _fields[key] : null;
        }

        public void AddEnum(Type type)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("The specified type is not allowed");
            }
            
            SetField(type.Name, new EnumWrapper(type));
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuTableDynamicMetaObject(parameter, BindingRestrictions.Empty, this);
        }

        private class LuTableDynamicMetaObject : DynamicMetaObject
        {
            public LuTableDynamicMetaObject(Expression expression, BindingRestrictions restrictions, object value) : base(expression, restrictions, value)
            {
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var getExpression = Expression.Call(Expression.Convert(Expression, LimitType), GetMethodInfo, Expression.Constant(binder.Name));
                var bindingRestrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                return new DynamicMetaObject(getExpression, bindingRestrictions);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                var getExpression = Expression.Call(Expression.Convert(Expression, LimitType), SetMethodInfo, Expression.Constant(binder.Name), Expression.Convert(value.Expression, typeof(object)));
                var bindingRestrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                return new DynamicMetaObject(getExpression, bindingRestrictions);
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
            {
                var getExpression = Expression.Call(Expression.Convert(Expression, LimitType), GetMethodInfo, indexes[0].Expression);
                var bindingRestrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                return new DynamicMetaObject(getExpression, bindingRestrictions);
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
            {
                var getExpression = Expression.Call(Expression.Convert(Expression, LimitType), SetMethodInfo, indexes[0].Expression, Expression.Convert(value.Expression, typeof(object)));
                var bindingRestrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                return new DynamicMetaObject(getExpression, bindingRestrictions);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                var memberExpression = Expression.Call(Expression.Convert(Expression, LimitType), GetMethodInfo, Expression.Constant(binder.Name));
                var invokeExpression = Expression.Dynamic(new LuInvokeBinder(new CallInfo(args.Length)), typeof(object), new[] { memberExpression }.Concat(args.Select(x => x.Expression)));
                var bindingRestrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                return new DynamicMetaObject(invokeExpression, bindingRestrictions);
            }
        }
    }
}