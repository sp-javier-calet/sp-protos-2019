using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using System;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class SinglePropertyWriter : AbstractPropertyWriter
    {
        public SinglePropertyWriter(string propName, object value, Type propType) : base(propName, "Single", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            writer.Write((float)this.value);
        }
    }
}