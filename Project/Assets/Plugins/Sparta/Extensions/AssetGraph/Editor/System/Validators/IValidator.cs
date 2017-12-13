using System;
using UnityEngine;
using UnityEditor;

using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace AssetBundleGraph
{

    /**
	 * IValidator is an interface which validates incoming assets.
	 * Subclass of IValidator must have CustomValidator attribute.
	 */
    public interface IValidator
    {

        /**
		 * Tells the validator if this object should be validated or is an exception.
		 */
        bool ShouldValidate(object asset);

        /**
		 * Validates incoming asset.
		 */
        bool Validate(object asset);

        /**
		 * When the validation fails you can try to recover in here and return if it is recovered
		 */
        bool TryToRecover(object asset);

        /**
		 * When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
		 */
        string ValidationFailed(object asset);

        /**
		 * Draw Inspector GUI for this Validator.
		 */
        void OnInspectorGUI(Action onValueChanged);

        /**
		 * Serialize this Validator to JSON using JsonUtility.
		 */
        string Serialize();
    }

    /**
	 * Used to declare the class is used as a IModifier. 
	 * Classes with CustomModifier attribute must implement IModifier interface.
	 */
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomValidator : Attribute
    {
        private string m_name;
        private Type m_validateFor;

        /**
		 * Name of Validator appears on GUI.
		 */
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /**
		 * Type of asset Validator validates.
		 */
        public Type For
        {
            get
            {
                return m_validateFor;
            }
        }

        /**
		 * CustomValidator declares the class is used as a IValidator.
		 * @param [in] name 	 Name of Validator appears on GUI.
		 * @param [in] modifyFor Type of asset Validator validates.
		 */
        public CustomValidator(string name, Type validateFor)
        {
            m_name = name;
            m_validateFor = validateFor;
        }
    }

    public class ValidatorUtility
    {
        private static Dictionary<Type, Dictionary<string, string>> s_attributeClassNameMap;

        public static Dictionary<string, string> GetAttributeClassNameMap(Type targetType)
        {

            UnityEngine.Assertions.Assert.IsNotNull(targetType);

            if(s_attributeClassNameMap == null)
            {
                s_attributeClassNameMap = new Dictionary<Type, Dictionary<string, string>>();
            }

            if(!s_attributeClassNameMap.Keys.Contains(targetType))
            {
                var map = new Dictionary<string, string>();

                var builders = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t != typeof(IValidator))
                    .Where(t => typeof(IValidator).IsAssignableFrom(t));

                foreach(var type in builders)
                {
                    CustomValidator attr =
                        type.GetCustomAttributes(typeof(CustomValidator), false).FirstOrDefault() as CustomValidator;

                    if(attr != null && attr.For == targetType)
                    {
                        if(!map.ContainsKey(attr.Name))
                        {
                            map[attr.Name] = type.FullName;
                        }
                        else
                        {
                            Debug.LogWarning("Multiple CustomValidator class with the same name/type found. Ignoring " + type.Name);
                        }
                    }
                }
                s_attributeClassNameMap[targetType] = map;
            }
            return s_attributeClassNameMap[targetType];
        }

        public static string GetValidatorGUIName(IValidator m)
        {
            CustomValidator attr =
                m.GetType().GetCustomAttributes(typeof(CustomValidator), false).FirstOrDefault() as CustomValidator;
            return attr.Name;
        }

        public static string GetValidatorGUIName(string className)
        {
            var type = Type.GetType(className);
            if(type != null)
            {
                CustomValidator attr =
                    Type.GetType(className).GetCustomAttributes(typeof(CustomValidator), false).FirstOrDefault() as CustomValidator;
                if(attr != null)
                {
                    return attr.Name;
                }
            }
            return string.Empty;
        }

        public static string GUINameToClassName(string guiName, Type targetType)
        {
            var map = GetAttributeClassNameMap(targetType);

            if(map.ContainsKey(guiName))
            {
                return map[guiName];
            }

            return null;
        }

        public static Type GetValidatorTargetType(IValidator v)
        {
            CustomValidator attr =
                v.GetType().GetCustomAttributes(typeof(CustomValidator), false).FirstOrDefault() as CustomValidator;
            UnityEngine.Assertions.Assert.IsNotNull(attr);
            return attr.For;
        }

        public static Type GetValidatorTargetType(string className)
        {
            var type = Type.GetType(className);
            if(type != null)
            {
                CustomValidator attr =
                    Type.GetType(className).GetCustomAttributes(typeof(CustomValidator), false).FirstOrDefault() as CustomValidator;
                if(attr != null)
                {
                    return attr.For;
                }
            }
            return null;
        }

        public static bool HasValidCustomValidatorAttribute(Type t)
        {
            CustomValidator attr =
                t.GetCustomAttributes(typeof(CustomValidator), false).FirstOrDefault() as CustomValidator;

            if(attr != null)
            {
                return !string.IsNullOrEmpty(attr.Name) && attr.For != null;
            }
            return false;
        }

        public static IValidator CreateValidator(NodeData node, BuildTarget target)
        {
            return CreateValidator(node, BuildTargetUtility.TargetToGroup(target));
        }

        public static IValidator CreateValidator(NodeData node, BuildTargetGroup targetGroup)
        {

            var data = node.InstanceData[targetGroup];
            var className = node.ScriptClassName;
            Type dataType = null;

            if(!string.IsNullOrEmpty(className))
            {
                dataType = Type.GetType(className);
            }

            if(data != null && dataType != null)
            {
                return JsonUtility.FromJson(data, dataType) as IValidator;
            }

            return null;
        }

        public static IValidator CreateValidator(string guiName, Type targetType)
        {
            var className = GUINameToClassName(guiName, targetType);
            if(className != null)
            {
                return (IValidator)Assembly.GetExecutingAssembly().CreateInstance(className);
            }
            return null;
        }

        public static IValidator CreateValidator(string className)
        {

            if(className == null)
            {
                return null;
            }

            Type t = Type.GetType(className);
            if(t == null)
            {
                return null;
            }

            if(!HasValidCustomValidatorAttribute(t))
            {
                return null;
            }

            return (IValidator)Assembly.GetExecutingAssembly().CreateInstance(className);
        }
    }
}
