﻿
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class StringPropertyReader : AbstractPropertyReader
    {
        public StringPropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return (string)value;
        }
    }
}