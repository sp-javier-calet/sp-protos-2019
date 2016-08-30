using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class StringPropertyWriter : AbstractPropertyWriter
    {
        public StringPropertyWriter(string propName, object value, Type propType) : base(propName, "String", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            writer.Write((string)this.value);
        }
    }
}