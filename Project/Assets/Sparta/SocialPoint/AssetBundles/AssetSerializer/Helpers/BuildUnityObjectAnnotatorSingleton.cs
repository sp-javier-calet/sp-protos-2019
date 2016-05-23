using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SocialPoint.AssetSerializer.Utils;

namespace SocialPoint.AssetSerializer.Helpers
{
    // Adhoc class to pass generic parameters to events
    public class LinkActionArguments
    {
        public enum LinkActionType
        {
            LINK_OBJECT_TO_LIST = 1,
            LINK_OBJECT_TO_ARRAY,
            LINK_OBJECT_TO_PROPERTY,
            LINK_OBJECT_TO_FIELD}

        ;

        public LinkActionType actionType;
        public object instanceObject;
        public int containerPosition;
        public FieldInfo fieldInfo;
        
        public int refObjectId;
    }

    public static class BuildUnityObjectAnnotatorSingleton
    {
        public static bool IsBuilding { get; private set; }

        static readonly Dictionary<int, UnityEngine.Object> assetidmapperDict = new Dictionary<int, UnityEngine.Object>();

        static readonly List<LinkActionArguments> linkActionsList = new List<LinkActionArguments>();

        static BuildUnityObjectAnnotatorSingleton()
        {
            Clear();
        }

        // Building Bundles Phase

        public static void Clear()
        {
            IsBuilding = false;
            assetidmapperDict.Clear();
            linkActionsList.Clear();
        }

        public static void BeginBuilding()
        {
            if(IsBuilding)
                throw new Exception("BuildUnityObjectAnnotatorSingleton instance is already building.");
            IsBuilding = true;
        }

        public static void EndBuilding()
        {
            if(!IsBuilding)
                throw new Exception("BuildUnityObjectAnnotatorSingleton instance wasn't building.");
            IsBuilding = false;
        }

        public static void AddObject(UnityEngine.Object obj)
        {
            if(!IsBuilding)
                throw new Exception("BuildUnityObjectAnnotatorSingleton - AddObject. instance is not building.");

            int objId = obj.GetInstanceID();

            assetidmapperDict[objId] = obj;
        }

        public static void AddObjectAndID(UnityEngine.Object obj, int objId)
        {
            if(!IsBuilding)
                throw new Exception("BuildUnityObjectAnnotatorSingleton - AddObject. instance is not building.");

            assetidmapperDict[objId] = obj;
        }

        public static void PrepareSceneForSerialization(ref UnityEngine.Object[] serializedAssets,
                                                         ref int[] serializedAssetIDs)
        {
            var assets = new List<UnityEngine.Object>();
            var assetIDs = new List<int>();

            var itr = assetidmapperDict.GetEnumerator();
            while(itr.MoveNext())
            {
                var entry = itr.Current;
                assets.Add(entry.Value);
                assetIDs.Add(entry.Key);
            }
            itr.Dispose();

            serializedAssets = assets.ToArray();
            serializedAssetIDs = assetIDs.ToArray();
        }

        // Reading Bundles Phase

        public static void ReadMapperFromSceneContainer(UnityEngine.Object[] serializedAssets,
                                                         int[] serializedAssetIDs)
        {
            if(IsBuilding)
                throw new Exception("BuildUnityObjectAnnotatorSingleton instance is building.");

            for(int i = 0; i < serializedAssets.Length; ++i)
            {
                int assetId = serializedAssetIDs[i];
                UnityEngine.Object asset = serializedAssets[i];

                assetidmapperDict[assetId] = asset;
            }
        }

        public static void AddLinkAction(LinkActionArguments linkAction)
        {
            linkActionsList.Add(linkAction);
        }

        public static void LinkActions()
        {
            for(int i = 0, linkActionsListCount = linkActionsList.Count; i < linkActionsListCount; i++)
            {
                LinkActionArguments linkAction = linkActionsList[i];
                UnityEngine.Object refObject;
                assetidmapperDict.TryGetValue(linkAction.refObjectId, out refObject);
                if(TypeUtils.CompareToNull(refObject))
                    throw new Exception("BuildUnityObjectAnnotatorSingleton referenced object not in the map.");
                switch(linkAction.actionType)
                {
                case LinkActionArguments.LinkActionType.LINK_OBJECT_TO_FIELD:
                    {
                        object instance = linkAction.instanceObject;
                        FieldInfo classFieldInfo = linkAction.fieldInfo;
                        classFieldInfo.SetValue(instance, refObject);
                    }
                    break;
                case LinkActionArguments.LinkActionType.LINK_OBJECT_TO_ARRAY:
                    {
                        var arr = (IList)linkAction.instanceObject;
                        arr[linkAction.containerPosition] = refObject;
                    }
                    break;
                case LinkActionArguments.LinkActionType.LINK_OBJECT_TO_LIST:
                    {
                        object instance = linkAction.instanceObject;
                        Type instanceType = instance.GetType();
                        PropertyInfo listItemProp = instanceType.GetProperty("Item");
                        listItemProp.SetValue(instance, refObject, new object[] {
                            linkAction.containerPosition
                        });
                    }
                    break;
                default:
                {
                    throw new Exception("BuildUnityObjectAnnotatorSingleton link action type not supported.");
                }
                }
            }

            linkActionsList.Clear();
        }
    }
}

