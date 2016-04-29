using UnityEngine;
using System;
using System.Reflection;


namespace SocialPoint.Tool.Server
{
    public class ToolServiceParameters
    {
        /**
         * Base properties
         */
        public string commandName;
        

        /**
         * Get a derived ToolServiceParameter class instance from its json serialized contents
         */
        public static T Instantiate<T>(string jsonContent) where T : ToolServiceParameters
        {
            return (T)Instantiate(jsonContent, typeof(T));
        }
        
        public static ToolServiceParameters Instantiate(string jsonContent, Type parametersType)
        {
            if(parametersType != typeof(ToolServiceParameters) && !parametersType.IsSubclassOf(typeof(ToolServiceParameters)))
            {
                throw new Exception(String.Format("Invalid parametersType={0}", parametersType.FullName));
            }
            
            // Equivalent of JsonMapper.ToObject<MyJsonMappedDataClass> (serializedJsonText)
            MethodInfo toObjMethod = Utils.GetJsonMapperToObjGeneric(parametersType);
            ToolServiceParameters instance = null;
            try
            {
                instance = (ToolServiceParameters)toObjMethod.Invoke(null, new object[] {jsonContent});
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
            
            return instance;
        }
        
        /**
         * Debug method
         */
        public void PrintPublicFields()
        {
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach(FieldInfo field in fields)
            {
                Debug.Log(field.Name + ": " + field.GetValue(this));
            }
        }
    }
}