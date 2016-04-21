using UnityEngine;
using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class UInt64PropertyWriter : AbstractPropertyWriter
    {
        public UInt64PropertyWriter(string propName, object value, Type propType) : base(propName, "UInt64", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            // Writting UInt64 is currently not supported.
            string wrnMsg = string.Format( "<color=yellow>UInt64 is not currently suported for serialization. Conside using Int32 Instead." +
                                          "(prop_name: {0})</color>", this.propName );
            Debug.LogWarning(wrnMsg);

            writer.Write((ulong)this.value);
        }
    }
}