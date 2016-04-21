using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class SerializableDecimalPropertyWriter : AbstractPropertyWriter
    {
        public SerializableDecimalPropertyWriter(string propName, object value, Type propType) : base(propName, "SerializableDecimal", value, propType)
        {
        }
        
        override public void WriteValueObject(JsonWriter writer)
        {
            SerializableDecimal sd = this.value as SerializableDecimal;
            writer.Write(sd.value.ToString());
        }
    }
}