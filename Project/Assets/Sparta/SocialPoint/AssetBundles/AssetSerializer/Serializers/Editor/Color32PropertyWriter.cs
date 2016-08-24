using System;
using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Color32PropertyWriter : AbstractPropertyWriter
    {
        public Color32PropertyWriter(string propName, object value, Type propType) : base(propName, "Color32", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            Color32 color = (Color32)this.value;
	        
            writer.WriteObjectStart();
	        
            writer.WritePropertyName("r");
            writer.Write(color.r);
	        
            writer.WritePropertyName("g");
            writer.Write(color.g);
	        
            writer.WritePropertyName("b");
            writer.Write(color.b);
	        
            writer.WritePropertyName("a");
            writer.Write(color.a);
	        
            writer.WriteObjectEnd();
        }
    }
}