using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class EnumPropertyReader : AbstractPropertyReader
    {
        public EnumPropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return Enum.Parse(propType, (string)value);
        }
    }
}