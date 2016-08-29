using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class QuaternionPropertyReader : AbstractPropertyReader
    {
        public QuaternionPropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return new Quaternion((float)(double)value["x"], (float)(double)value["y"], (float)(double)value["z"], (float)(double)value["w"]);
        }
    }
}