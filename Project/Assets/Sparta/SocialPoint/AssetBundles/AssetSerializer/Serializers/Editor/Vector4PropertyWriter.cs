using UnityEngine;
using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Vector4PropertyWriter : AbstractPropertyWriter
    {
        public Vector4PropertyWriter(string propName, object value, Type propType) : base(propName, "Vector4", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            Vector4 vector = (Vector4)value;
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(vector.x);
	        
            writer.WritePropertyName("y");
            writer.Write(vector.y);
	        
            writer.WritePropertyName("z");
            writer.Write(vector.z);
	        
            writer.WritePropertyName("w");
            writer.Write(vector.w);
	        
            writer.WriteObjectEnd();
        }
    }
}