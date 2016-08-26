using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SocialPoint.AssetSerializer.Helpers;
using SocialPoint.AssetSerializer.Utils;
using BM.Extensions;

namespace BundleManagerJSON
{
    public sealed class JSONHelper
    {
        // When serializing prefabs and removing components, use this instances instead of directly modifying prefabs
        // This way we do not touch the root prefabs
        private static Dictionary<string, string> copyedPrefabs;

        static JSONHelper()
        {
            copyedPrefabs = new Dictionary<string, string>();
            Clear();
        }

        public static void Clear()
        {
            if(BundleManagerJSON.JSONConfig.UsePrefabCopies)
            {
                AssetDatabase.DeleteAsset(GetTmpPrefabPath());
                AssetDatabase.Refresh();
            }
            copyedPrefabs.Clear();
        }

        public static StringBuilder TouchMetaFile(string assetPath)
        {
            string assetMetaPath = assetPath + ".meta";
            // This will modify the .meta file(inocuos) for unity to detect the changes
            StringBuilder sb = null;
            using(StreamReader sr = new StreamReader (assetMetaPath))
            {
                sb = new StringBuilder(sr.ReadToEnd());
            }
            sb = TouchMetaFile(assetPath, sb);
            return sb;
        }

        public static StringBuilder TouchMetaFile(string assetPath, StringBuilder currentMetaContent)
        {
            string assetMetaPath = assetPath + ".meta";
            using(FileStream fs = new FileStream (assetMetaPath, FileMode.Truncate, FileAccess.Write))
            {
                var sw = new StreamWriter(fs);
                if(currentMetaContent[currentMetaContent.Length - 1] == '\n')
                {
                    currentMetaContent.Remove(currentMetaContent.Length - 1, 1);
                }
                else
                {
                    currentMetaContent.Append('\n');
                }
                sw.Write(currentMetaContent);
                sw.Flush();
            }
            return currentMetaContent;
        }

        public static void AddPrefabCopy(string assetPath)
        {
            var tmpPrefabPath = GetTmpPrefabPath();
            if(!copyedPrefabs.ContainsKey(assetPath))
            {

                // If UsePrefabCopies is enabled, instead of using the assetPath prefab to serialize, a tmp copy will be instead
                if(BundleManagerJSON.JSONConfig.UsePrefabCopies)
                {
                    if(AssetDatabase.LoadAssetAtPath(tmpPrefabPath, typeof(UnityEngine.Object)) == null)
                    {
                        AssetDatabase.CreateFolder(Path.GetDirectoryName(tmpPrefabPath), Path.GetFileName(tmpPrefabPath));
                    }
                    var copyPrefabPath = tmpPrefabPath + "/" + Path.GetFileName(assetPath);
                    AssetDatabase.CopyAsset(assetPath, copyPrefabPath);
                    copyedPrefabs[assetPath] = copyPrefabPath;
                    AssetDatabase.Refresh();
                }
                // Else, the same prefab is stored to be loaded and serialized
                else
                {
                    copyedPrefabs[assetPath] = assetPath;
                }
            }
        }

        public static GameObject FindPrefab(string assetPath)
        {
            string copyPrefabPath = "";
            if(copyedPrefabs.TryGetValue(assetPath, out copyPrefabPath))
            {
                return AssetDatabase.LoadAssetAtPath(copyPrefabPath, typeof(GameObject)) as GameObject;
            }
            else
            {
                return null;
            }
        }

        public static string GetTmpPrefabPath()
        {
            return BMDataAccessor.GetUniquePlatformAssetDir("JSON_PREFAB");
        }

        public static string GetTmpSerializedDataPath()
        {
            return BMDataAccessor.GetUniquePlatformAssetDir("JSON_Data");
        }

        /// <summary>
        /// Builds the JSON serialized file from bundle data.
        /// </summary>
        /// <returns>The list of JSON files generated.</returns>
        /// <param name="data">Data.</param>
        public static List<string> BuildJSONFromBundleData(BundleData data)
        {
            SerializerLogger.Enable(true);
            SerializerLogger.Clear();

            L("BuildJSONFromBundleData");
            List<string> dataAssets = new List<string>();

            //Serialization won't work with multiple scenes per bundle so we have to check that and fail:
            //the reason is that we have to open the scene, modify it and NOT SAVE IT. The changes will be lost for multiple scenes.
            int sceneAssetCount = 0;

            foreach(string assetPath in data.includs)
            {
                if(IsSceneAssetPath(assetPath))
                {
                    sceneAssetCount++;
                }
                
                if(sceneAssetCount > 1)
                {
                    SerializerLogger.LogError(String.Format("The bundle '{0}' contains more than one scene and this is not allowed for serialization.",
                                                            data.name));
                    SerializerLogger.ShowLogAndExceptIfNeeded();
                    SerializerLogger.Clear();
                }

                string jsonFileGUID = BuildJSONFromAssetPath(assetPath);
                if(jsonFileGUID != String.Empty)
                {
                    dataAssets.Add(jsonFileGUID);
                }
            }

            SerializerLogger.Enable(false);
            
            return dataAssets;
        }

        /// <summary>
        /// Builds the JSON serialized file from asset path.
        /// </summary>
        /// <returns>The GUID of the json file.</returns>
        /// <param name="assetPath">Asset path.</param>
        public static string BuildJSONFromAssetPath(string assetPath)
        {
            L("BuildJSONFromAssetPath: " + assetPath);
            if(IsPrefabAssetPath(assetPath))
            {
                return BuildJSONForPrefab(assetPath);
            }
            else if(IsSceneAssetPath(assetPath))
            {
                return BuildJSONForScene(assetPath);
            }
            else
            {
                //Ignore this asset as it doesn't need to be serialized
                SerializerLogger.LogMsg(String.Format("Pass Serialization. The root assetPath {0} doesn't needs to be serialized" +
                    " because is neither a scene nor a prefab.", assetPath));
                SerializerLogger.ShowLogAndExceptIfNeeded();
                SerializerLogger.Clear();
            }
            return String.Empty;
        }

        public static bool IsSceneAssetPath(string assetPath)
        {
            return assetPath.EndsWith(".unity");
        }
        
        public static bool IsPrefabAssetPath(string assetPath)
        {
            return assetPath.EndsWith(".prefab");
        }

        /// <summary>
        /// Builds the JSON for a prefab object.
        /// </summary>
        /// <returns>The GUID of the json file.</returns>
        /// <param name="assetPath">Asset path.</param>
        public static string BuildJSONForPrefab(string assetPath)
        {
            L("BuildJSONForPrefab: " + assetPath);
			
            string jsonFileGUID = "";
			
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            if(prefab != null)
            {
                // Serialize 
                string serialized = SerializerHelper.SerializeObject(prefab);
                SerializerLogger.ShowLogAndExceptIfNeeded();
                SerializerLogger.Clear();

                //Only save the serialized data if there is any
                if(serialized != String.Empty)
                {
                    // Store the string 
                    string serializedFilePath = SaveSerializedData(serialized, assetPath);
                    jsonFileGUID = AssetDatabase.AssetPathToGUID("Assets/" + serializedFilePath);
                    // 
                    AddPrefabCopy(assetPath);
                    var prefabCopy = FindPrefab(assetPath);
                    // Disconnect scene prefabs prior to remove behaviours or changes will be propagated and custom properties will be lost
                    foreach(var pInst in GetPrefabInstancesInScene (prefabCopy))
                        PrefabUtility.DisconnectPrefabInstance(pInst);
                    ComponentHelper.RemoveAllBehaviours(prefabCopy, true);
                }
                else
                {
                    SerializerLogger.LogMsg(String.Format("Pass Serialization. The root assetPath {0} doesn't needs to be serialized" +
                        " because no serializable components could be found.", assetPath));
                    SerializerLogger.ShowLogAndExceptIfNeeded();
                    SerializerLogger.Clear();

                    return String.Empty;
                }
            }
			
            return jsonFileGUID;
        }
		
        /// <summary>
        /// Builds the JSON for a scene.
        /// </summary>
        /// <returns>The GUID of the json file.</returns>
        /// <param name="assetPath">Asset path.</param>
        public static string BuildJSONForScene(string assetPath)
        {
            L("BuildJSONForScene: " + assetPath);
			
            string jsonFileGUID = "";
			
            if(UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {

                // Check if scene file exists, it could have been moved
                string fullAssetPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
                if(!File.Exists(assetPath))
                {
                    if(EditorUtility.DisplayDialog("Cannot find scene",
                                                   "The scene included in this bundle has been moved or does no longer exist.\n" +
                                                   "Please remove the included scene from the bundle and add it again.\n\n" +
                                                   fullAssetPath,
                                                   "Ok"))
                    {
                        throw new UnityException(String.Format("The bundle scene could not be found. (path:'{0}')", fullAssetPath));
                    }
                }


                // Load scene and find root GameObjects
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(assetPath);

                // Custom bundle build procedure prior to scene serialization
                BuildHelper.CallBuildBundleProcedures<BBPPreSceneSerialization>();

                SerializerLogger.SetCurrentScene(Path.GetFileNameWithoutExtension(assetPath));

                UnityEngine.Object[] rootObjects = GetRootObjectsIntoScene();

                // Serialize Scene
                string serialized = SerializerHelper.SerializeScene(rootObjects, true, BundleManagerJSON.JSONConfig.SerializeNonCustomGameObjects);

                SerializerLogger.ShowLogAndExceptIfNeeded();
                SerializerLogger.SetCurrentScene("");
                SerializerLogger.Clear();

                //Only save the serialized data if there is any
                if(serialized != String.Empty)
                {
                    // Store the string 
                    string serializedFilePath = SaveSerializedData(serialized, assetPath);

                    Debug.Log(string.Format("<color=cyan>serializedFilePath: {0}</color>", serializedFilePath));

                    jsonFileGUID = AssetDatabase.AssetPathToGUID("Assets/" + serializedFilePath);
					
                    // Add json data as a MonoBehaviour container
                    GameObject jsonContainer = new GameObject("SERIALIZATION_JSON_CONTAINER");
                    JSONSceneContainer jsonSceneContainer = jsonContainer.AddComponent<JSONSceneContainer>();
                    TextAsset serializedTextAsset = AssetDatabase.LoadAssetAtPath(Path.Combine("Assets", serializedFilePath), typeof(TextAsset)) as TextAsset;
                    jsonSceneContainer.serializationJSONData = serializedTextAsset;

                    List<int> rootGameObjectIDs = new List<int>();
                    List<GameObject> rootGameObjects = new List<GameObject>();
                    for(int i = 0; i < rootObjects.Length; i++)
                    {
                        if(rootObjects[i] is GameObject)
                        {
                            GameObject rootGameObject = rootObjects[i] as GameObject;
                            rootGameObjects.Add(rootGameObject);
                            rootGameObjectIDs.Add(rootGameObject.GetInstanceID());
                        }
                    }
                    jsonSceneContainer.rootGameObjects = rootGameObjects.ToArray();
                    jsonSceneContainer.rootGameObjectIDs = rootGameObjectIDs.ToArray();
                    BuildUnityObjectAnnotatorSingleton.PrepareSceneForSerialization(ref jsonSceneContainer.serializedAssets, ref jsonSceneContainer.serializedAssetIDs);

                    // Remove the Monobehavior
                    ComponentHelper.RemoveAllBehaviorFromList(rootObjects, true);
                }
                else
                {
                    SerializerLogger.LogMsg(String.Format("Pass Serialization. The root assetPath {0} doesn't needs to be serialized" +
                        " because no serializable components could be found.", assetPath));
                    SerializerLogger.ShowLogAndExceptIfNeeded();
                    SerializerLogger.Clear();
                }
            }
            else
            {
                throw new UnityException("Target scene can't be loaded");
            }
			
            return jsonFileGUID;
        }

        /// <summary>
        /// Reassigns the behaviours from bundle data.
        /// </summary>
        /// <param name="data">Data.</param>
        public static void ReassignBehavioursFromBundleData(BundleData data)
        {
            L("ReassignBehavioursFromBundleData");
            foreach(string assetPath in data.includs)
            {
                ReassignBehavioursFromAssetPath(assetPath);
            }
        }

        /// <summary>
        /// Reassigns the behaviours from asset path.
        /// </summary>
        /// <param name="assetPath">Asset path.</param>
        public static void ReassignBehavioursFromAssetPath(string assetPath)
        {
            L("ReassignBehavioursFromAssetPath: " + assetPath);
            if(assetPath.EndsWith(".prefab"))
            {
                ReassignBehavioursForPrefab(assetPath);
            }
            else if(assetPath.EndsWith(".unity"))
            {
                ReassignBehavioursForScene(assetPath);
            }
            //This asset does not need to be deserialized
        }		

        /// <summary>
        /// Reassigns the behaviours for a prefab.
        /// </summary>
        /// <param name="assetPath">Asset path.</param>
        public static void ReassignBehavioursForPrefab(string assetPath)
        {
            L("ReassignBehavioursForPrefab: " + assetPath);
            string jsonFilePath = assetPath;
            if(jsonFilePath.StartsWith("Assets/"))
            {
                jsonFilePath = jsonFilePath.Substring(jsonFilePath.IndexOf("/") + 1);
            }
            jsonFilePath = GetTmpSerializedDataPath() + "/" + jsonFilePath.Replace(".prefab", "_JSON_Data.json");
			
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath(jsonFilePath, typeof(TextAsset)) as TextAsset;
            if(textAsset != null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                ComponentHelper.DeserializeObject(prefab, textAsset.text);
            }
            else
            {
                //This asset is not serialized and do not need to reassign behaviours
                Debug.Log(String.Format("BM_JSON_Data not found. The asset {0} doesn't need to be serialized.", assetPath));
            }
        }
		
        /// <summary>
        /// Reassigns the behaviours for a scene.
        /// </summary>
        /// <param name="scenePath">Scene path.</param>
        public static void ReassignBehavioursForScene(string scenePath)
        {
            L("ReassignBehavioursForScene: " + scenePath);

            JSONSceneContainer jsonSceneContainer = GetJSONContainerIntoScene();
            if(jsonSceneContainer != null)
            {
                ComponentHelper.DeserializeScene(jsonSceneContainer);
                UnityEngine.Object.DestroyImmediate(jsonSceneContainer.gameObject);
            }
            else
            {
                //This asset is not serialized and do not need to reassign behaviours
                Debug.Log(String.Format("BM_JSON_Data not found. The asset {0} doesn't need to be serialized.", scenePath));
            }
        }

        /// <summary>
        /// Gets the root objects into scene.
        /// </summary>
        /// <returns>The root objects into scene.</returns>
        public static UnityEngine.Object[] GetRootObjectsIntoScene()
        {
            List<UnityEngine.Object> rootObjects = new List<UnityEngine.Object>();

            //This method allows to get the root game objects in the current scene including
            //the inactive ones
            var prop = new HierarchyProperty(HierarchyType.GameObjects);
            var expanded = new int[0];
            while(prop.Next (expanded))
            {
                rootObjects.Add(prop.pptrValue as GameObject);
            }

            return rootObjects.ToArray();
        }

        /// <summary>
        /// Gets all prefab instances of a root prefab in the current scene
        /// </summary>
        /// <returns>The array of GmaeObjects that are prefab instances in the scene</returns>
        public static GameObject[] GetPrefabInstancesInScene(GameObject rootPrefab)
        {
            List<GameObject> instancesList = new List<GameObject>();
            var rootObjects = GetRootObjectsIntoScene();
            for(int i = 0; i < rootObjects.Length; ++i)
            {
                if(rootObjects[i] is GameObject)
                {
                    GetPrefabInstances(rootObjects[i] as GameObject, rootPrefab, ref instancesList);
                }
            }
            return instancesList.ToArray();
        }

        static void GetPrefabInstances(GameObject parentGo, GameObject rootPrefab, ref List<GameObject> instancesList)
        {
            if(PrefabUtility.GetPrefabParent(parentGo) == rootPrefab)
            {
                instancesList.Add(parentGo);
            }
            else
            {
                for(int i = 0; i < parentGo.transform.childCount; ++i)
                {
                    GetPrefabInstances(parentGo.transform.GetChild(i).gameObject, rootPrefab, ref instancesList);
                }
            }
        }

        /// <summary>
        /// Gets the JSON serialized container into scene.
        /// </summary>
        /// <returns>The JSON container into scene.</returns>
        public static JSONSceneContainer GetJSONContainerIntoScene()
        {
            UnityEngine.Object[] rootObjects = GetRootObjectsIntoScene();
            for(int i = 0; i < rootObjects.Length; i++)
            {
                if(rootObjects[i] is GameObject)
                {
                    GameObject o = rootObjects[i] as GameObject;
                    JSONSceneContainer container = o.GetComponent<JSONSceneContainer>();
                    if(container != null)
                    {
                        return container;
                    }
                }
            }

            return null;
        }

        public static void RemoveSerializedDataFile()
        {
            AssetDatabase.DeleteAsset(GetTmpSerializedDataPath());
        }

        /**********************************************************************
		 * Internal functions
		 */

        private static string SaveSerializedData(string serialized, string sourceFilePath)
        {
            string tmpDataPath = GetTmpSerializedDataPath().Replace("Assets/", "");
            string serializedFilePath = sourceFilePath;
            if(serializedFilePath.StartsWith("Assets/"))
            {
                serializedFilePath = serializedFilePath.Substring(serializedFilePath.IndexOf("/") + 1);
            }
			
            if(serializedFilePath.EndsWith(".prefab"))
            {
                serializedFilePath = tmpDataPath + "/" + serializedFilePath.Replace(".prefab", "_JSON_Data.json");
            }
            else if(serializedFilePath.EndsWith(".unity"))
            {
                serializedFilePath = tmpDataPath + "/" + serializedFilePath.Replace(".unity", "_JSON_Data.json");
            }
            else
            {
                throw new UnityException(string.Format("The asset path '{0}' must be '.prefab' or '.unity'", serializedFilePath));
            }
			
            Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath + "/" + serializedFilePath));
            File.WriteAllText(Application.dataPath + "/" + serializedFilePath, serialized);
            AssetDatabase.Refresh();
			
            return serializedFilePath;
        }

        private static void L(string m)
        {
            Debug.Log(string.Format("<color=yellow>JSONHelper</color> <color=white>'{0}'</color>", m));
        }
    }
}
