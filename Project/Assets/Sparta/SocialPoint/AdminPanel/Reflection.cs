#if ADMIN_PANEL

using System;
using System.Reflection;
using SocialPoint.Base;

namespace SocialPoint.AdminPanel
{
    public static class Reflection
    {
        const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        public static R GetPrivateField<T, R>(object instance, string fieldName)
        {
            FieldInfo field = typeof(T).GetField(fieldName, bindFlags);
            if(field == null)
            {
                Log.w(string.Format("AdminPanel Error. No '{0}` field in class {1}", fieldName, instance.GetType()));
            }

            return field != null ? (R)field.GetValue(instance) : default(R);
        }

        public static R GetPrivateProperty<T, R>(object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            PropertyInfo property = typeof(T).GetProperty(fieldName, bindFlags);
            if(property == null)
            {
                Log.w(string.Format("AdminPanel Error. No '{0}` property in class {1}", fieldName, instance.GetType()));
            }

            return property != null ? (R)property.GetValue(instance, null) : default(R);
        }

        static MethodInfo GetMethod<T>(string methodName)
        {
            // FIXME Does not work for overloaded methods
            MethodInfo method = typeof(T).GetMethod(methodName, bindFlags);

            if(method == null)
            {
                throw new Exception("Private method not found");
            }

            return method;
        }

        public static void CallPrivateVoidMethod<T>(object instance, string methodName, params object[] parameters)
        {
            try
            {
                GetMethod<T>(methodName).Invoke(instance, parameters);
            }
            catch(Exception e)
            {
                Log.w(string.Format("AdminPanel Error. Error invoking '{0}` method in class {1}. Cause: {2}", methodName, instance.GetType(), e));
            }
        }

        public static R CallPrivateMethod<T, R>(object instance, string methodName, R defaultReturn, params object[] parameters)
        {
            try
            {
                return (R)GetMethod<T>(methodName).Invoke(instance, parameters);
            }
            catch(Exception e)
            {
                Log.w(string.Format("AdminPanel Error. Error invoking '{0}` method in class {1}. Cause: {2}", methodName, instance.GetType(), e));
                return defaultReturn;
            }
        }
    }
}

#endif
