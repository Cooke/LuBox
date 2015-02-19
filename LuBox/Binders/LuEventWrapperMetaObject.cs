namespace LuBox.Runtime
{
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class LuEventWrapperMetaObject : DynamicMetaObject
    {
        private readonly EventInfo _eventInfo;

        public LuEventWrapperMetaObject(Expression expression, EventInfo eventInfo, object instance)
            : base(expression, BindingRestrictions.Empty, instance)
        {
            _eventInfo = eventInfo;
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Invalid number of arguments");    
            }

            if (string.Equals(binder.Name, "Add", StringComparison.InvariantCultureIgnoreCase))
            {
                var addMethodInfo = _eventInfo.AddMethod;
                return CreateDynamicEventOperation(args, addMethodInfo);
            }
            else if (string.Equals(binder.Name, "Remove", StringComparison.InvariantCultureIgnoreCase))
            {
                var addMethodInfo = _eventInfo.RemoveMethod;
                return CreateDynamicEventOperation(args, addMethodInfo);
            }

            throw new NotImplementedException();
        }

        private DynamicMetaObject CreateDynamicEventOperation(DynamicMetaObject[] args, MethodInfo methodInfo)
        {
            BindingRestrictions eventRestriction = BindingRestrictions.GetInstanceRestriction(Expression.Property(Expression.Convert(Expression, typeof(LuEventWrapper)), "EventInfo"), _eventInfo);
            UnaryExpression instanceExpression = Expression.Convert(Expression.Property(Expression.Convert(Expression, typeof(LuEventWrapper)), "Instance"), _eventInfo.DeclaringType);
            DynamicMetaObject onlyArgument = args.Single();
            Expression argExpression = Expression.Dynamic(new LuConvertBinder(_eventInfo.EventHandlerType, false), _eventInfo.EventHandlerType, onlyArgument.Expression);
            return new DynamicMetaObject(RuntimeHelpers.EnsureObjectResult(Expression.Call(instanceExpression, methodInfo, argExpression)), eventRestriction);
        }
    }
}