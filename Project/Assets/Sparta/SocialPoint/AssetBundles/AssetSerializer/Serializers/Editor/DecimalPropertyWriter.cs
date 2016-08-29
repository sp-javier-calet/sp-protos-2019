using System;
using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class DecimalPropertyWriter : AbstractPropertyWriter
    {
        public DecimalPropertyWriter(string propName, object value, Type propType) : base(propName, "decimal", value, propType)
        {
        }

		override public void WriteValueObject(JsonWriter writer)
        {
            // Writting decimals is currently not supported.
            string wrnMsg = string.Format( "<color=yellow>decimal is not currently suported for serialization. Consider using custom class SerializableDecimal instead." +
                                          "(prop_name: {0})</color>", this.propName );
            Debug.LogWarning(wrnMsg);

            writer.Write(((decimal)this.value).ToString());
        }
    }
}