using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Color32PropertyReader : AbstractPropertyReader
    {
        public Color32PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return new Color32((byte)value["r"], (byte)value["g"], (byte)value["b"], (byte)value["a"]);
        }
    }
}