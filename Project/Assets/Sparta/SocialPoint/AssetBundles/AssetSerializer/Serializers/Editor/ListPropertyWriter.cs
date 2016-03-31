using System;
using System.Collections;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using System.Linq;
using SocialPoint.AssetSerializer.Utils;

namespace SocialPoint.AssetSerializer.Serializers
{
	public class ListPropertyWriter : AbstractPropertyWriter
	{
		/**
		 * Example of item type:
		 *     System.Collections.Generic.List`1[[System.Int32, mscorlib]]
		 */
        public ListPropertyWriter( string propName, object value, Type propType ) : base( propName, "List", value, propType )
		{
		}
		
		override public void WriteValueObject( JsonWriter writer )
		{
			writer.WriteArrayStart ();

            object[] objArr;
            if (value == null)
                objArr = new object[] {};
            else
                objArr = ((IEnumerable)value).Cast<object>().ToArray();

            Type objType = propType.GetGenericArguments () [0];

			for ( int i = 0; i < objArr.Length; i++ ) {
				string itemName = "list_item" + i;
                AbstractPropertyWriter itemWriter = WriterSerializerFactory.GetPropertyWriter( itemName, objArr[i], objType );

                itemWriter.WriteObject( writer );
			}
			writer.WriteArrayEnd ();
		}

		private string GetContainingItemTypeName( )
		{
            string listTypeName = TypeUtils.GetSerializedType (propType);

			int sidx = listTypeName.IndexOf ('[');
			int eidx = listTypeName.LastIndexOf (']');

			string subs = listTypeName.Substring (sidx, eidx - sidx);
			string containingItmTypeFullName = subs.TrimStart ('[').TrimEnd (']');

			Type containingItmType = Type.GetType (containingItmTypeFullName);

			return TypeUtils.GetRepresentativeType (containingItmType);
		}
	}
}