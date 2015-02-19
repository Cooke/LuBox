namespace LuBox.Runtime
{
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;

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

        public EventInfo EventInfo { get { return _eventInfo; } }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuEventWrapperMetaObject(parameter, _eventInfo, this);
        }
    }
}