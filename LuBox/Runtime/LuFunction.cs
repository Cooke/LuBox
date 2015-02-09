namespace LuBox.Runtime
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class LuFunction : IDynamicMetaObjectProvider
    {
        private readonly Delegate _delegate;

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
    }
}