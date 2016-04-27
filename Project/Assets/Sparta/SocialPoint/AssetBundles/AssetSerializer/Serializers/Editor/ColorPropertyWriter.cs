using System;
using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class ColorPropertyWriter : AbstractPropertyWriter
    {
        public ColorPropertyWriter(string propName, object value, Type propType) : base(propName, "Color", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            Color color = (Color)this.value;
	        
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