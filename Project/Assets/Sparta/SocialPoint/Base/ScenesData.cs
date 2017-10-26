using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SocialPoint.Base
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif

    //IMPORTANT: This assets is Updated only if you SAVE and RESTART UNITY!!!!
    public class ScenesData : ScriptableObject
    {
        const string FolderName = "ScenesData/";
        const string FileName = "ScenesData";

        #if UNITY_EDITOR
        const string FileExtension = ".asset";
        static readonly string ContainerPath = ConfigPaths.SpartaConfigResourcesPath + FolderName;
        static readonly string AssetFullPath = ContainerPath + FileName + FileExtension;
        #endif

        static ScenesData _instance;

        public static ScenesData Instance
        {
            get
            {
                return GetInstance();
            }
        }
            
        #if !UNITY_EDITOR
        void OnEnable()
        {
            GetInstance();
        }
        #endif
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #endif
        static ScenesData GetInstance()
        {
            if(_instance == null)
            {
                #if UNITY_EDITOR
                _instance = AssetDatabase.LoadAssetAtPath<ScenesData>(AssetFullPath);
                #else
                _instance = Resources.Load<ScenesData>(FolderName + FileName);
                #endif

                if(_instance == null)
                {
                    // If not found, autocreate the asset object.
                    _instance = CreateInstance<ScenesData>();

                    #if UNITY_EDITOR
                    CreateAsset();
                    #endif
                }

                #if UNITY_EDITOR
                UpdateData();
                #endif
            }
            return _instance;
        }

        #if UNITY_EDITOR
        static void CreateAsset()
        {
            if(!Directory.Exists(ContainerPath))
            {
                Directory.CreateDirectory(ContainerPath);
            }

            if(!File.Exists(AssetFullPath))
            {
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(AssetFullPath);
                AssetDatabase.CreateAsset(_instance, assetPath);
                AssetDatabase.SaveAssets();
            }
        }

        static public void UpdateData()
        {
            UpdateSceneNames();
            UpdateAsset();
        }

        static void UpdateSceneNames()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            var namesList = new List<string>();

            for(int i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if(scene.enabled)
                {
                    var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    namesList.Add(sceneName);
                }
            }
                
            _instance._scenesNames = namesList.ToArray();
        }

        static void UpdateAsset()
        {
            EditorUtility.SetDirty(_instance);
        }

        #endif

        [SerializeField]
        string[] _scenesNames;

        public string[] ScenesNames
        {
            get
            {
                return _scenesNames;
            }
        }
    }
}
