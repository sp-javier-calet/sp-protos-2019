using UnityEngine;
using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Vector3PropertyWriter : AbstractPropertyWriter
    {
        public Vector3PropertyWriter(string propName, object value, Type propType) : base(propName, "Vector3", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            Vector3 vector = (Vector3)value;
            writer.WriteObjectStart();

            writer.WritePropertyName("x");
            writer.Write(vector.x);
	        
            writer.WritePropertyName("y");
            writer.Write(vector.y);
	        
            writer.WritePropertyName("z");
            writer.Write(vector.z);
	        
            writer.WriteObjectEnd();
        }
    }
}