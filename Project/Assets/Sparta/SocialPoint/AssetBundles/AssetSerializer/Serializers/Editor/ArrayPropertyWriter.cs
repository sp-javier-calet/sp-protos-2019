using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using System.Linq;
using SocialPoint.AssetSerializer.Utils;

namespace SocialPoint.AssetSerializer.Serializers
{
	public sealed class ArrayPropertyWriter : AbstractPropertyWriter
	{
		public ArrayPropertyWriter( string propName, object value, Type propType ) : base( propName, "Array", value, propType )
		{
		}
		
		override public void WriteValueObject( JsonWriter writer )
		{
			writer.WriteArrayStart ();

            object[] objArr;
            if (value == null)
                objArr = new object[] {};
            else
                objArr = ((System.Collections.IEnumerable)value).Cast<object>().ToArray();

            Type objType = propType.GetElementType();

			for ( int i = 0; i < objArr.Length; i++ ) {
				string itemName = "array_item" + i;
                AbstractPropertyWriter itemWriter = WriterSerializerFactory.GetPropertyWriter( itemName, objArr[i], objType );

                itemWriter.WriteObject( writer );
			}
			writer.WriteArrayEnd ();
		}

        private string GetContainingItemTypeName( )
        {
            return TypeUtils.GetRepresentativeType(propType.GetElementType ());
        }
	}
}