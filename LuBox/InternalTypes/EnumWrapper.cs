using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    public class EnumWrapper : IDynamicMetaObjectProvider
    {
        private readonly Dictionary<string, object> _mapping;

        public EnumWrapper(Type en)
        {
            var names = Enum.GetNames(en);
            var values = Enum.GetValues(en);
            _mapping = names.Select((x, i) => new {Key = x, Value = values.GetValue(i)}).ToDictionary(x => x.Key, x => x.Value);
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuEnumWrapperMetaObject(parameter, BindingRestrictions.Empty, this);
        }

        public object GetValue(string key)
        {
            return _mapping.ContainsKey(key) ? _mapping[key] : null;
        }

        private class LuEnumWrapperMetaObject : DynamicMetaObject 
        {
            public LuEnumWrapperMetaObject(Expression expression, BindingRestrictions restrictions, object value) : base(expression, restrictions, value)
            {
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var callExpression = Expression.Call(Expression.Convert(Expression, typeof (EnumWrapper)),
                    typeof (EnumWrapper).GetMethod("GetValue"),
                    Expression.Constant(binder.Name));
                return new DynamicMetaObject(callExpression, BindingRestrictions.GetTypeRestriction(Expression, typeof(EnumWrapper)));
            }
        }
    }
}
