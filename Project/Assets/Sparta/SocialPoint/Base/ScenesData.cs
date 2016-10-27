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

    public class ScenesData : ScriptableObject
    {
        const string FileName = "ScenesData";
        const string FileExtension = ".asset";
        const string ContainerPath = "Assets/Sparta/Config/ScenesData/Resources/";

        const string ScenesDataAssetPath = ContainerPath + FileName + FileExtension;

        static ScenesData _instance;

        public static ScenesData Instance
        {
            get
            {
                return GetInstance();
            }
        }

        [SerializeField]
        string[] _scenesNames = new string[0];

        public string[] ScenesNames
        {
            get
            {
                return _scenesNames;
            }
        }

        #if UNITY_EDITOR
        //Static constructor needed to run upon editor initialization (UnityEditor.InitializeOnLoad)
        static ScenesData()
        {
            GetInstance();
        }
        #endif

        static ScenesData GetInstance()
        {
            if(_instance == null)
            {
                #if UNITY_EDITOR
                _instance = AssetDatabase.LoadAssetAtPath<ScenesData>(ScenesDataAssetPath);
                #else
                _instance = Resources.Load(FileName) as ScenesData;
                #endif

                if(_instance == null)
                {
                    // If not found, autocreate the asset object.
                    _instance = CreateInstance<ScenesData>();

                    #if UNITY_EDITOR
                    CreateAsset();
                    #endif
                }

                UpdateData();
            }
            return _instance;
        }

        static public void UpdateData()
        {
            #if UNITY_EDITOR
            UpdateSceneNames();
            UpdateAsset();
            #endif
        }

        #if UNITY_EDITOR
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

            Instance._scenesNames = namesList.ToArray();
        }

        static void UpdateAsset()
        {
            EditorUtility.SetDirty(Instance);
        }

        static void CreateAsset()
        {
            if(!Directory.Exists(ContainerPath))
            {
                Directory.CreateDirectory(ContainerPath);
            }

            string assetPath = ScenesDataAssetPath;
            AssetDatabase.CreateAsset(Instance, assetPath);
            AssetDatabase.SaveAssets();
        }
        #endif
    }
}
