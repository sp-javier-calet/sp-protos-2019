using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class DoublePropertyWriter : AbstractPropertyWriter
    {
        public DoublePropertyWriter(string propName, object value, Type propType) : base(propName, "double", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            writer.Write((double)this.value);
        }
    }
}