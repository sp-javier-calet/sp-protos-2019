using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class Vector2PropertyReader : AbstractPropertyReader
    {
        public Vector2PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return new Vector2((float)(double)value["x"], (float)(double)value["y"]);
        }
    }
}