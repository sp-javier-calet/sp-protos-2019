using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class BooleanPropertyWriter : AbstractPropertyWriter
    {
        public BooleanPropertyWriter(string propName, object value, Type propType) : base(propName, "Boolean", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            writer.Write((bool)this.value);
        }
    }
}