using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using LuBox.Runtime;

namespace LuBox.Binders
{
    internal class BinderProvider
    {
        private readonly ConcurrentDictionary<InvokeMemberBinderCacheKey, LuInvokeMemberBinder> _invokeMemberBinders = new ConcurrentDictionary<InvokeMemberBinderCacheKey, LuInvokeMemberBinder>();
        private readonly ConcurrentDictionary<string, LuGetMemberBinder> _getMemberBinders = new ConcurrentDictionary<string, LuGetMemberBinder>();
        private readonly ConcurrentDictionary<string, LuSetMemberBinder> _setMemberBinders = new ConcurrentDictionary<string, LuSetMemberBinder>();

        public LuInvokeMemberBinder GetInvokeMemberBinder(string memberName, CallInfo callInfo)
        {
            var cacheKey = new InvokeMemberBinderCacheKey(memberName, callInfo);

            LuInvokeMemberBinder binder;
            _invokeMemberBinders.TryGetValue(cacheKey, out binder);

            if (binder == null)
            {
                binder = new LuInvokeMemberBinder(memberName, false, callInfo);
                _invokeMemberBinders.TryAdd(cacheKey, binder);
            }

            return binder;
        }

        public CallSiteBinder GetGetMemberBinder(string memberName)
        {
            LuGetMemberBinder binder;
            _getMemberBinders.TryGetValue(memberName, out binder);

            if (binder == null)
            {
                binder = new LuGetMemberBinder(memberName);
                _getMemberBinders.TryAdd(memberName, binder);
            }

            return binder;
        }

         public CallSiteBinder GetSetMemberBinder(string memberName)
        {
            LuSetMemberBinder binder;
            _setMemberBinders.TryGetValue(memberName, out binder);

            if (binder == null)
            {
                binder = new LuSetMemberBinder(memberName);
                _setMemberBinders.TryAdd(memberName, binder);
            }

            return binder;
        }

        private class InvokeMemberBinderCacheKey
        {
            private readonly string _memberName;
            private readonly CallInfo _callInfo;

            public InvokeMemberBinderCacheKey(string memberName, CallInfo callInfo)
            {
                _memberName = memberName;
                _callInfo = callInfo;
            }

            public override int GetHashCode()
            {
                return 0x28000000 ^ _memberName.GetHashCode() ^ _callInfo.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var mci = obj as InvokeMemberBinderCacheKey;
                return mci != null && mci._memberName == _memberName && mci._callInfo.Equals(_callInfo);
            }
        }
    }
}
