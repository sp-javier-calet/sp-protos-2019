using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using System.Reflection;
using SocialPoint.AssetSerializer.Helpers;

namespace SocialPoint.AssetSerializer.Serializers
{
	public sealed class ListPropertyReader : AbstractPropertyReader
	{
        public ListPropertyReader(JsonData propDef) : base(propDef)
		{
		}
		
		override public object ReadValueObject()
		{
			// Create generic list
            Type itemType = propType.GetGenericArguments()[0];
			var instance = Activator.CreateInstance( propType );
			
			MethodInfo add_methodInfo = propType.GetMethod( "Add" );
			
			// Loop items
            JsonData items = value;
			for ( int i = 0; i < items.Count; i++ ) {

                AbstractPropertyReader itemReader = ReaderSerializerFactory.GetPropertyReader( itemType, items[i] );

                object itemValue = itemReader.ReadValueObject();
				
                if (itemReader is UnityObjectReferencePropertyReader && itemValue != null) {
                    LinkActionArguments linkAction = new LinkActionArguments();
                    linkAction.actionType = LinkActionArguments.LinkActionType.LINK_OBJECT_TO_LIST;
                    linkAction.instanceObject = instance;
                    linkAction.containerPosition = i;
                    linkAction.refObjectId = (int)itemValue;
                    BuildUnityObjectAnnotatorSingleton.AddLinkAction(linkAction);

					add_methodInfo.Invoke( instance, new object[] { null } );
                } else {

					add_methodInfo.Invoke( instance, new object[] { itemValue } );
				}
			}
			
			return instance;
		}
	}
}
