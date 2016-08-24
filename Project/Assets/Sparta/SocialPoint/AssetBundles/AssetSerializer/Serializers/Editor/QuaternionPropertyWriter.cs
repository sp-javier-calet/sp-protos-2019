using System;
using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class QuaternionPropertyWriter : AbstractPropertyWriter
    {
        public QuaternionPropertyWriter(string propName, object value, Type propType) : base(propName, "Quaternion", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            Quaternion vector = (Quaternion)value;
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