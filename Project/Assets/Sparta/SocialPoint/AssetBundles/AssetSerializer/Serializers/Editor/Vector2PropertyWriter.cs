using UnityEngine;
using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Vector2PropertyWriter : AbstractPropertyWriter
    {
        public Vector2PropertyWriter(string propName, object value, Type propType) : base(propName, "Vector2", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            Vector2 vector = (Vector2)value;
            writer.WriteObjectStart();

            writer.WritePropertyName("x");
            writer.Write(vector.x);
	        
            writer.WritePropertyName("y");
            writer.Write(vector.y);

            writer.WriteObjectEnd();
        }
    }
}