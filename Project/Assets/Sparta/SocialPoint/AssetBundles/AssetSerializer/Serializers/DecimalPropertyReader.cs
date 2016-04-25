using System;
using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class DecimalPropertyReader : AbstractPropertyReader
    {
        public DecimalPropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            // Writting decimals is currently not supported.
            string wrnMsg = string.Format( "<color=yellow>decimal is not currently suported for serialization. Consider using custom class SerializableDecimal instead." );
            Debug.LogWarning(wrnMsg);

            decimal val;
            if (decimal.TryParse((string)value, out val))
                return val;
            else 
                throw new Exception("Decimal could not be deserialized");
        }
    }
}