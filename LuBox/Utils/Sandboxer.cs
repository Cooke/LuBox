using System;

namespace LuBox.Utils
{
    using System.Reflection;

    public static class Sandboxer
    {
        public static bool IsReflectionType(Type type)
        {
            if (typeof(Type).IsAssignableFrom(type))
            {
                return true;
            }
           
            if (type.Namespace != null && type.Namespace.StartsWith("System.Reflection"))
            {
                return true;
            }

            return false;
        }

        public static void ThrowIfReflectionMember(MemberInfo memberInfo)
        {
            MethodInfo methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null && IsReflectionType(methodInfo.ReturnType))
            {
                throw new LuSandboxException(string.Format("Access to method with return a type of {0} is not allowed", methodInfo.ReturnType));
            }

            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null && IsReflectionType(propertyInfo.PropertyType))
            {
                throw new LuSandboxException(string.Format("Access to property with type {0} is not allowed", propertyInfo.PropertyType));
            }

            FieldInfo filedInfo = memberInfo as FieldInfo;
            if (filedInfo != null && IsReflectionType(filedInfo.FieldType))
            {
                throw new LuSandboxException(string.Format("Access to field of type {0} is not allowed", filedInfo.FieldType));
            }
        }

        public static void ThrowIfReflectionType(Type type)
        {
            if (IsReflectionType(type))
            {
                throw new LuSandboxException(string.Format("Type {0} is not allowed", type));
            }
        }
    }
}
