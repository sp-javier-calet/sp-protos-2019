using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using UnityEngine;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class LayerMaskPropertyReader : AbstractPropertyReader
    {
        public LayerMaskPropertyReader(JsonData propDef) : base(propDef)
        {
        }
        
        override public object ReadValueObject()
        {
            return (LayerMask)(int)value;
        }
    }
}
