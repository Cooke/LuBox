using LuBox.Binders;

namespace LuBox.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class LuFunction : IDynamicMetaObjectProvider
    {
        private readonly Delegate _delegate;
        private readonly Dictionary<Type, Delegate> _signatures = new Dictionary<Type, Delegate>(); 

        public LuFunction(Delegate @delegate)
        {
            _delegate = @delegate;
        }

        public Delegate Delegate
        {
            get { return _delegate; }
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new LuFunctionMetaObject(parameter, BindingRestrictions.Empty, this, _delegate.GetType());
        }

        public bool TryGetCompatibleSignature(Type requestedSignature, out Delegate del)
        {
            return _signatures.TryGetValue(requestedSignature, out del);
        }

        public Delegate AddSignature(Delegate del)
        {
            _signatures.Add(del.GetType(), del);
            return del;
        }
    }
}