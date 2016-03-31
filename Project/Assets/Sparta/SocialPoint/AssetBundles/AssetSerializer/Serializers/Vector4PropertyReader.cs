using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class Vector4PropertyReader : AbstractPropertyReader
    {
        public Vector4PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return new Vector4((float)(double)value["x"], (float)(double)value["y"], (float)(double)value["z"], (float)(double)value["w"]);
        }
    }
}