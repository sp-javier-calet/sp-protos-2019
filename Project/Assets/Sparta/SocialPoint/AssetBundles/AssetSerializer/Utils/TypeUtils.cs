using System;
using System.Reflection;
using UnityEngine;
using SocialPoint.AssetSerializer.Exceptions;
using SocialPoint.Utils;

namespace SocialPoint.AssetSerializer.Utils
{
	public sealed class TypeUtils
	{
        const string kEditorSuffix = "-Editor";
        const string kGenericList = "System.Collections.Generic.List";

		public static string LIST_COMPLETE_TYPE_NAME = "System.Collections.Generic.List`1";

        private static string WRITER_TMPL = "SocialPoint.AssetSerializer.Serializers.{{TYPE_NAME}}PropertyWriter";
        private static string READER_TMPL = "SocialPoint.AssetSerializer.Serializers.{{TYPE_NAME}}PropertyReader";

		/// <summary>
		/// Gets formatted type string so GetType always find the type when deserializing.
		/// For Lists and all non class types it just returns a Namespaced type name.
		/// For classes, arrays and enums, it returns the Namespaced class name plus the assembly where it lives.
		/// </summary>
		/// <returns>The serialized type.</returns>
		/// <param name="itemType">Item type.</param>
		public static string GetSerializedType( Type itemType )
		{
			// Lists are accepted in Unity and it's serializable type name should be treated a bit differently
			if (itemType.IsGenericType && GetCompleteTypeName (itemType) == LIST_COMPLETE_TYPE_NAME) {

				Type genericType = itemType.GetGenericArguments () [0];
				string genericTypeSerialized = GetSerializedType (genericType);

				string listTypeName = LIST_COMPLETE_TYPE_NAME + "[[" + genericTypeSerialized + "]]";

				return listTypeName;
			} else if (itemType.IsArray) {

				Type referencedItemType = itemType.GetElementType ();
				string referencedItemTypeSerialized = GetSerializedType (referencedItemType);

				string[] typeAndAssembly = ParseTypeAndAssembly (referencedItemTypeSerialized);
				if (typeAndAssembly.Length == 2)

					return string.Join (", ", new string[] { typeAndAssembly[0] + "[]", typeAndAssembly[1] });
				else

					return typeAndAssembly[0] + "[]";
            } else if (itemType.IsClass || itemType.IsEnum || itemType.Assembly.GetName().Name != "mscorlib") {
                
                string itemTypeName = GetCompleteTypeName (itemType);
                string itemTypeAssemblyName = itemType.Assembly.GetName ().Name;
                
                return string.Join (", ", new string[] { itemTypeName, itemTypeAssemblyName });
            } else {
				string itemTypeName = GetCompleteTypeName (itemType);

				return itemTypeName;
			}
		}

		/// <summary>
		/// Gets the namespace plust the name of the type. (ie."System.Int32")
		/// </summary>
		/// <returns>The complete type name.</returns>
		/// <param name="itemType">Item type.</param>
		public static string GetCompleteTypeName( Type itemType )
		{
            string name;

            if (itemType.IsEnum)
            {
                name = itemType.FullName;
            }
            else 
            {
                name = itemType.Name;
				
                //	Nested class
                if(itemType.DeclaringType != null)
                {
                    string declaringType = itemType.DeclaringType.Name;
                    name = string.Join ("+", new string[] { declaringType, name });
                }

                string nmspace = itemType.Namespace;
                if (nmspace != null && nmspace != string.Empty)
                {
                    name = string.Join (".", new string[] { nmspace, name });
                }
            }
            return name;
		}

        /// <summary>
        /// Gets a retpresentative type to get a reader/writer later on
        /// </summary>
        /// <param name="itemType">Item type.</param>
        public static string GetRepresentativeType( Type itemType )
        {
            string reprType = "";

            if (itemType.IsArray)
            {
                reprType = "Array";
            } else if (itemType.IsEnum)
            {
                reprType = "Enum";
            } else if (itemType.IsClass && StringUtils.StartsWith(itemType.FullName, kGenericList))
            {
                reprType = "List";
            } else if (itemType.IsClass && (itemType == typeof(UnityEngine.Object) || itemType.IsSubclassOf(typeof(UnityEngine.Object))))
            {
                reprType = "UnityObjectReference";
            } else if (itemType.IsClass && FindTypeRWInPropperAssembly(itemType))
            {
                reprType = TypeNameToFactoryTypeName(itemType.Name);
            } else if (itemType.IsClass && !itemType.IsGenericType)
            {
                reprType = "Class";
            } else if (FindTypeRWInPropperAssembly(itemType))
            {
                reprType = TypeNameToFactoryTypeName(itemType.Name);
            }
            else
            {
                throw new SerializationTypeNotSupportedException( itemType );
            }
            return reprType;
        }

		public static string[] ParseTypeAndAssembly( string typeName )
		{
			int sindx = typeName.LastIndexOf(", ");

			// No assembly specified
			if (sindx == -1) {
				return new string[] { typeName };
			} else {
				string typeSplitName = typeName.Substring(0, sindx);
				string assemblySplitName = typeName.Substring(sindx + 2);

				if (assemblySplitName.Contains("]")) {
					// Inner assembly definition, not the actual checked type
					return new string[] { typeName };
				} else {
					return new string[] { typeSplitName, assemblySplitName };
				}
			}
		}

        public static string TypeNameToFactoryTypeName( string typeName )
        {
            return char.ToUpper(typeName[0]) + typeName.Substring(1);
        }

        public static bool FindTypeRWInPropperAssembly( Type itemType )
        {
            //Writters are found on the Editor assemlby while Readers are found on the runtime
            string typeName = TypeNameToFactoryTypeName( itemType.Name );
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            if (StringUtils.EndsWith(currentAssembly.GetName ().Name, kEditorSuffix)) {

                string writerTypeString = WRITER_TMPL.Replace("{{TYPE_NAME}}", typeName);
                if (currentAssembly.GetType(writerTypeString) == null)
                    return false;
            } else {

                string readerTypeString = READER_TMPL.Replace("{{TYPE_NAME}}", typeName);
                if (currentAssembly.GetType(readerTypeString) == null)
                    return false;
            }

            return true;
        }

		public static bool CompareToNull( object instance, bool exceptOnMissing=false )
        {
            if (instance is UnityEngine.Object)
            {
                try
                {
                    #pragma warning disable 168
                    var checkExistence = (instance as UnityEngine.Object).name;
                    #pragma warning restore 168
                } catch (Exception e)
                {
                    if (e is UnassignedReferenceException || e is NullReferenceException)
                        return true;
                    else if (e is MissingReferenceException) {
                        if (exceptOnMissing)
                            throw new SerializationMissingReferenceException("");
                    }
                } 
                return false;
            } else
            {
                return object.Equals(instance, null);
            }
        }
	}
}

