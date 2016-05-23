using System;
using System.Collections.Generic;
using System.Reflection;
using SocialPoint.AssetSerializer.Helpers;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class ArrayPropertyReader : AbstractPropertyReader
    {
        public ArrayPropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            Type referencedItemType = propType.GetElementType();

            // Create generic list
            Type genericListType = typeof(List<>);
            Type listType = genericListType.MakeGenericType(referencedItemType);
            var instance = Activator.CreateInstance(listType);

            MethodInfo add_methodInfo = listType.GetMethod("Add");
            MethodInfo toArray_methodInfo = listType.GetMethod("ToArray");

            // Loop items
            JsonData items = value;
            var tmpLinkActions = new List<LinkActionArguments>();
            for(int i = 0; i < items.Count; i++)
            {

                AbstractPropertyReader itemReader = ReaderSerializerFactory.GetPropertyReader(referencedItemType, items[i]);

                object itemValue = itemReader.ReadValueObject();

                if(itemReader is UnityObjectReferencePropertyReader && itemValue != null)
                {
                    var linkAction = new LinkActionArguments();
                    linkAction.actionType = LinkActionArguments.LinkActionType.LINK_OBJECT_TO_ARRAY;
                    linkAction.containerPosition = i;
                    linkAction.refObjectId = (int)itemValue;
                    tmpLinkActions.Add(linkAction);

                    add_methodInfo.Invoke(instance, new object[] { null });
                }
                else
                {

                    add_methodInfo.Invoke(instance, new [] { itemValue });
                }
            }

            object arrayInstance = toArray_methodInfo.Invoke(instance, null);

            for(int i = 0, tmpLinkActionsCount = tmpLinkActions.Count; i < tmpLinkActionsCount; i++)
            {
                LinkActionArguments linkAction = tmpLinkActions[i];
                linkAction.instanceObject = arrayInstance;
                BuildUnityObjectAnnotatorSingleton.AddLinkAction(linkAction);
            }

            // Return result as array
            return arrayInstance;
        }
    }
}
