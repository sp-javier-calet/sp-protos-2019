using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class ColorPropertyReader : AbstractPropertyReader
    {
        public ColorPropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return new Color((float)(double)value["r"], (float)(double)value["g"], (float)(double)value["b"], (float)(double)value["a"]);
        }
    }
}