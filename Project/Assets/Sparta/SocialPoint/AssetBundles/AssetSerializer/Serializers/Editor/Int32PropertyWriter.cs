using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Int32PropertyWriter : AbstractPropertyWriter
    {
        public Int32PropertyWriter(string propName, object value, Type propType) : base(propName, "Int32", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            writer.Write((int)this.value);
        }
    }
}