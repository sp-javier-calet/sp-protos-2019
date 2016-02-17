using UnityEngine;
using System.Reflection;

namespace SocialPoint.AdminPanel
{
    public static class Reflection
    {
        public static R GetPrivateField<R>(object instance, string fieldName) where R : class
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            
            FieldInfo field = instance.GetType().GetField(fieldName, bindFlags);
            if(field == null)
            {
                Debug.LogWarning(string.Format("AdminPanel Error. No '{0}` field in class {1}", fieldName, instance.GetType()));
            }
            return field != null ? field.GetValue(instance) as R : null;
        }
    }
}
