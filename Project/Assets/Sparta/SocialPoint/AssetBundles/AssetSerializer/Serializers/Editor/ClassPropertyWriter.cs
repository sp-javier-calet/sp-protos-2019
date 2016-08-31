using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using System.Reflection;
using System;

namespace SocialPoint.AssetSerializer.Serializers
{
	public sealed class ClassPropertyWriter : AbstractPropertyWriter
	{
		public ClassPropertyWriter( string propName, object value, Type propType ) : base( propName, "Class", value, propType )
		{
		}
		
		override public void WriteValueObject( JsonWriter writer )
		{
			writer.WriteObjectStart();

			writer.WritePropertyName( "fields" );

            if (value == null)
                writer.Write(null);

            else {
                writer.WriteArrayStart ();

				// TODO: There's no support for single serializable fields, either the class is or not at all

				if (propType.IsSerializable) {
					FieldInfo[] fields = propType.GetFields( ( ( BindingFlags.Public | BindingFlags.Instance ) & 
					                                          ~( BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.GetField | BindingFlags.SetField ) ) );

	    			for ( int i = 0; i < fields.Length; i++ ) {
						FieldInfo field = fields[i];

                        AbstractPropertyWriter fieldWriter = WriterSerializerFactory.GetPropertyWriter( field.Name, field.GetValue( value ), field.FieldType );
	    				if ( fieldWriter != null ) {
	    					fieldWriter.WriteObject( writer );
	    				}
	    			}
				}
    			writer.WriteArrayEnd ();
            }

			writer.WriteObjectEnd();
		}
	}
}