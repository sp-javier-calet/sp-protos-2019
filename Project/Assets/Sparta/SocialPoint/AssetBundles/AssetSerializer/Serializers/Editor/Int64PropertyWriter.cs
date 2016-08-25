using System;
using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Int64PropertyWriter : AbstractPropertyWriter
    {
        public Int64PropertyWriter(string propName, object value, Type propType) : base(propName, "Int64", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            // Writting Int64 is currently not supported.
            string wrnMsg = string.Format( "<color=yellow>Int64 is not currently suported for serialization." +
                                          "(prop_name: {0})</color>", this.propName );
            Debug.LogWarning(wrnMsg);

            writer.Write((long)this.value);
        }
    }
}