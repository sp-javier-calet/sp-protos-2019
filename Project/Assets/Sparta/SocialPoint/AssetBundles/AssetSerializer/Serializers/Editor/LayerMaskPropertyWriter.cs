using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using UnityEngine;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class LayerMaskPropertyWriter : AbstractPropertyWriter
    {
        public LayerMaskPropertyWriter(string propName, object value, Type propType) : base(propName, "LayerMask", value, propType)
        {
        }
        
        override public void WriteValueObject(JsonWriter writer)
        {
            writer.Write(((LayerMask)this.value).value);
        }
    }
}

