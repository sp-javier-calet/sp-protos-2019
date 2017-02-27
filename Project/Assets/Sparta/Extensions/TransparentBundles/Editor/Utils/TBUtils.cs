using System;
using System.Reflection;
using LitJson;

namespace SocialPoint.TransparentBundles
{
    public static class TBUtils
    {
        /**
        * Get the closed generic method 'T ToObj<T> (string content)' for the 
        * type T of toObjType.
        * Allows direct json deserialisation into class toObjType
        */
        public static MethodInfo GetJsonMapperToObjGeneric(Type toObjType)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            MethodInfo[] matchingMethods = typeof(JsonMapper).GetMethods(flags);
            for(int i = 0; i < matchingMethods.Length; ++i)
            {
                MethodInfo method = matchingMethods[i];
                ParameterInfo[] parameters = method.GetParameters();
                if(method.Name == "ToObject" &&
                   parameters.Length == 1 &&
                   parameters[0].ParameterType == typeof(string) &&
                   method.IsGenericMethod)
                {
                    Type[] genericMethodTypes = method.GetGenericArguments();
                    if(genericMethodTypes.Length == 1 &&
                       genericMethodTypes[0] == method.ReturnType)
                    {
                        return method.MakeGenericMethod(toObjType);
                    }
                }
            }

            throw new Exception("Could not find a 'public static T JsonMapper.ToObj<T> (string)' definition");
        }
    }
}
