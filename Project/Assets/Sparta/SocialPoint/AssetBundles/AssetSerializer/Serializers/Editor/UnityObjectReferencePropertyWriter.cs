using UnityEngine;
using UnityEditor;
using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using SocialPoint.AssetSerializer.Utils;
using SocialPoint.AssetSerializer.Helpers;
using SocialPoint.AssetSerializer.Exceptions;


namespace SocialPoint.AssetSerializer.Serializers
{
    public class UnityObjectReferencePropertyWriter : AbstractPropertyWriter
    {
        public UnityObjectReferencePropertyWriter(string propName, object value, Type propType) : base(propName, "UnityObjectReference", value, propType)
        {
        }

        override public void WriteValueObject(JsonWriter writer)
        {
            try {
                if (TypeUtils.CompareToNull(value, true)) {
                    writer.Write(null);
                }
                else {
                    UnityEngine.Object obj = value as UnityEngine.Object;

                    // Pure prefab properties are not allowed
                    if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab)
                        throw new SerializationPurePrefabNotSupportedException(obj.name, propName);

                    writer.Write( obj.GetInstanceID() );

                    BuildUnityObjectAnnotatorSingleton.AddObject(obj);
                }
            } catch (SerializationMissingReferenceException) {
				SerializerLogger.LogWarning(String.Format("Script field {0} is missing, it's value will be serialized to null.", propName));
                writer.Write(null);
            } catch (SerializationPurePrefabNotSupportedException e) {
                SerializerLogger.LogError(e.ToString());
                writer.Write(null);
            }
        }
    }
}

