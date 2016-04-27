using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class Int64PropertyReader : AbstractPropertyReader
    {
        public Int64PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            // Writting Int64 is currently not supported.
            string wrnMsg = string.Format( "<color=yellow>Int64 is not currently suported for serialization." );
            Debug.LogWarning(wrnMsg);

            return long.Parse(value.ToString());
        }
    }
}