using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class Vector3PropertyReader : AbstractPropertyReader
    {
        public Vector3PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return new Vector3((float)(double)value["x"], (float)(double)value["y"], (float)(double)value["z"]);
        }
    }
}