using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class UInt64PropertyReader : AbstractPropertyReader
    {
        public UInt64PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            // Writting UInt64 is currently not supported.
            string wrnMsg = string.Format( "<color=yellow>UInt64 is not currently suported for serialization. Consider using Int32 instead." );
            Debug.LogWarning(wrnMsg);

            return ulong.Parse(value.ToString());
        }
    }
}