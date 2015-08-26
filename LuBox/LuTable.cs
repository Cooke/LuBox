using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static MethodInfo HasMethodInfo = typeof(LuTable).GetMethod("HasField");
        public static ConstructorInfo EmptyConstructorInfo = typeof (LuTable).GetConstructor(new Type[0]);
        public static ConstructorInfo ValuesConstructorInfo = typeof(LuTable).GetConstructor(new [] { typeof(IEnumerable<KeyValuePair<object, object>>) });

        private readonly IDictionary<object, object> _fields;

        public LuTable() : this(Enumerable.Empty<KeyValuePair<object, object>>())
        {
        }

        public LuTable(IEnumerable<KeyValuePair<object, object>> initials)
        {
            _fields = initials.ToDictionary(x => x.Key, x => x.Value);
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
            AddEnum(type, type.Name);
        }

        public void AddEnum(Type type, string name)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("The specified type is not allowed");
            }

            SetField(name, new EnumWrapper(type));
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
                var getExpression = Expression.Call(Expression.Convert(Expression, LimitType), GetMethodInfo, Expression.Convert(indexes[0].Expression, typeof(object)));
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

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                var emptyConstructorInfo = binder.Type.GetConstructor(new Type[0]);
                if (binder.Type != LimitType && emptyConstructorInfo != null && binder.Type.IsClass)
                {
                    var newExpression = Expression.New(binder.Type);
                    var varExpression = Expression.Variable(binder.Type);
                    var assignExpression = Expression.Assign(varExpression, newExpression);

                    var setProperties = binder.Type.GetProperties();
                    var setExpressions = new List<Expression>();
                    foreach (var propertyInfo in setProperties)
                    {
                        var targetExpression = Expression.Convert(Expression, LimitType);
                        var propertyNameConstExpression = Expression.Constant(propertyInfo.Name);
                        var hasFieldExpression = Expression.Call(targetExpression, HasMethodInfo, propertyNameConstExpression);
                        var getExpression = Expression.Call(targetExpression, GetMethodInfo, propertyNameConstExpression);
                        var propertyConvertExpression = Expression.Dynamic(new LuConvertBinder(propertyInfo.PropertyType), propertyInfo.PropertyType, getExpression);
                        var ifAssignExpression = Expression.IfThen(hasFieldExpression,
                            Expression.Assign(Expression.Property(varExpression, propertyInfo),
                                propertyConvertExpression));
                        setExpressions.Add(ifAssignExpression);
                    }

                    return new DynamicMetaObject(Expression.Block(new[] { varExpression }, new Expression[] { assignExpression }.Concat(setExpressions).Concat(new[] { varExpression})), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
                }
                else
                {
                    return binder.FallbackConvert(this);
                }
            }
        }

        public ICollection<object> GetKeys()
        {
            return _fields.Keys;
        }
    }
}