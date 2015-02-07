using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LuBox.Runtime
{
    internal class LuFunction : IDynamicMetaObjectProvider
    {
        private readonly Delegate _del;

        public LuFunction(Delegate del)
        {
            _del = del;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuMethodMetaObject(parameter, this, BindingRestrictions.Empty, this);
        }

        public Delegate Del
        {
            get { return _del; }
        }
    }

    internal class LuMethodMetaObject : DynamicMetaObject
    {
        private readonly LuFunction _luFunction;

        public LuMethodMetaObject(Expression expression, LuFunction luFunction, BindingRestrictions restrictions) : base(expression, restrictions)
        {
            _luFunction = luFunction;
        }

        public LuMethodMetaObject(Expression expression, LuFunction luFunction, BindingRestrictions restrictions, object value) : base(expression, restrictions, value)
        {
            _luFunction = luFunction;
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            return new DynamicMetaObject(Expression.Invoke(Expression.Constant(_luFunction.Del), args.Select(x => RuntimeHelpers.EnsureObjectResult(x.Expression))), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            binder.Type.GetMethod("")
        }
    }

    internal class LuEventWrapper : IDynamicMetaObjectProvider
    {
        private readonly EventInfo _eventInfo;
        private readonly object _instance;

        public LuEventWrapper(EventInfo eventInfo, object instance)
        {
            _eventInfo = eventInfo;
            _instance = instance;
        }

        public object Instance { get { return _instance; } }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuEventMetaObject(parameter, _eventInfo, this, this);
        }
    }

    internal class LuEventMetaObject : DynamicMetaObject
    {
        private readonly EventInfo _eventInfo;
        private readonly LuEventWrapper _luEventWrapper;

        public LuEventMetaObject(Expression expression, EventInfo eventInfo, LuEventWrapper luEventWrapper, object instance)
            : base(expression, BindingRestrictions.Empty, instance)
        {
            _eventInfo = eventInfo;
            _luEventWrapper = luEventWrapper;
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                if (binder.Name == "Add")
                {
                    //var dynamicMethod = new DynamicMethod();
                    //dynamicMethod.In
                    return
                        new DynamicMetaObject(
                            RuntimeHelpers.EnsureObjectResult(
                                Expression.Call(
                                    Expression.Convert(Expression.Constant(_luEventWrapper.Instance),
                                        _eventInfo.DeclaringType),
                                    _eventInfo.AddMethod,
                                    args.Select(x => Expression.Convert(x.Expression, _eventInfo.EventHandlerType)))),
                            BindingRestrictions.GetTypeRestriction(Expression, LimitType));
                }

                throw new NotImplementedException();
            }
    }

    internal class NuGetMemberBinder : GetMemberBinder
    {
        public NuGetMemberBinder(string name) : base(name, false)
        {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {            
            if (!target.HasValue)
            {
                return Defer(target);
            }

            const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
            var members = target.LimitType.GetMember(Name, flags);
            if (members.Length != 1)
            {
                throw new NotSupportedException();
            }
            
            var member = members[0];

            if (member.MemberType == MemberTypes.Event)
            {
                return
                    new DynamicMetaObject(
                        Expression.New(
                            typeof (LuEventWrapper).GetConstructor(new[] {typeof (EventInfo), typeof (object)}),
                            Expression.Constant(member), target.Expression), BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
                //return new DynamicMetaObject(
                //    RuntimeHelpers.EnsureObjectResult(
                //        Expression.New(typeof(LuEventWrapper).GetConstructor(new[] { typeof(EventInfo), typeof(object) }), Expression.Constant(member), target.Expression)),
                //    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }

            return new DynamicMetaObject(
                RuntimeHelpers.EnsureObjectResult(
                    Expression.MakeMemberAccess(Expression.Convert(target.Expression, member.DeclaringType), member)),
                BindingRestrictions.GetTypeRestriction(target.Expression,
                    target.LimitType));
        }
    }
}