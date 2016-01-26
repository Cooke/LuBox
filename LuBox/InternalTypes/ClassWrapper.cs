using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LuBox.Runtime;

namespace LuBox.InternalTypes
{
    internal class ClassWrapper : IDynamicMetaObjectProvider
    {
        private readonly Type _type;

        public ClassWrapper(Type type)
        {
            _type = type;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuClassWrapperMetaObject(parameter,
                BindingRestrictions.GetExpressionRestriction(
                    Expression.Equal(Expression.Field(Expression.Convert(parameter, typeof (ClassWrapper)), nameof(_type)), Expression.Constant(_type))), this,
                _type);
        }

        private class LuClassWrapperMetaObject : DynamicMetaObject 
        {
            private readonly Type _type;

            public LuClassWrapperMetaObject(Expression expression, BindingRestrictions restrictions, object value, Type type) : base(expression, restrictions, value)
            {
                _type = type;
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                ConstructorInfo[] constructorInfos = _type.GetConstructors();
                var sortedConstructorInfos = SignatureHelper.OrderSignatureMatches(args, constructorInfos.Cast<MethodBase>().ToArray());
                var constructorInfo = (ConstructorInfo) sortedConstructorInfos.First();

                var newExpression = Expression.New(constructorInfo, SignatureHelper.TransformArguments(args, constructorInfo));
                
                var argRestrictrions = args.Select(x => BindingRestrictions.GetTypeRestriction(x.Expression, x.LimitType));
                var argRestriction = RestrictionHelper.CombineRestrictions(argRestrictrions.ToArray());
                var restriction = RestrictionHelper.CombineRestrictions(argRestriction, Restrictions);

                return new DynamicMetaObject(newExpression, restriction);
            }
        }
    }
}
