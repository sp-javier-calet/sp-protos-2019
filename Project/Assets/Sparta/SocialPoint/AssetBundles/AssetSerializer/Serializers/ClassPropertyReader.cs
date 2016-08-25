using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using System.Reflection;
using SocialPoint.AssetSerializer.Helpers;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class ClassPropertyReader : AbstractPropertyReader
    {
        public ClassPropertyReader(JsonData propDef) : base(propDef)
        {
        }
        
        override public object ReadValueObject()
        {
            object instance;

            JsonData fieldListData = value["fields"];

            if(fieldListData == null || propType == null)
            {
                instance = null;
            }
            else
            {
                instance = Activator.CreateInstance(propType);

                for(int i = 0; i < fieldListData.Count; i++)
                {
                    JsonData fieldData = fieldListData[i];

                    string fieldTypeName = (string)fieldData["type"];
                    
                    AbstractPropertyReader fieldReader = ReaderSerializerFactory.GetPropertyReader(fieldTypeName, fieldData);

                    if(fieldReader != null)
                    {
                        string fieldName = (string)fieldData["name"];
                        object fieldValue = fieldReader.ReadValueObject();
                        FieldInfo fieldInfo = propType.GetField(fieldName);

                        if(fieldReader is UnityObjectReferencePropertyReader && fieldValue != null)
                        {
                            LinkActionArguments linkAction = new LinkActionArguments();
                            linkAction.actionType = LinkActionArguments.LinkActionType.LINK_OBJECT_TO_FIELD;
                            linkAction.instanceObject = instance;
                            linkAction.fieldInfo = fieldInfo;
                            linkAction.refObjectId = (int)fieldValue;
                            BuildUnityObjectAnnotatorSingleton.AddLinkAction(linkAction);

                            fieldInfo.SetValue(instance, null);
                        }
                        else
                        {
                            fieldInfo.SetValue(instance, fieldValue);
                        }
                    }
                }
            }

            return instance;
        }
    }
}
