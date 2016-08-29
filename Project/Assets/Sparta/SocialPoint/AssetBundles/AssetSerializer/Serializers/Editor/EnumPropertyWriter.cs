using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class EnumPropertyWriter : AbstractPropertyWriter
    {
        public EnumPropertyWriter(string propName, object value, Type propType) : base(propName, "Enum", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            writer.Write(value.ToString());
        }
    }
}