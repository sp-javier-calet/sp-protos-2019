using System.IO;
using SocialPoint.Base;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SocialPoint.IntroAnimation
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif

    public class IntroScreenData : ScriptableObject
    {
        const string FolderName = "IntroScreen/";
        const string FileName = "IntroScreen";

        #if UNITY_EDITOR
        const string FileExtension = ".asset";
        static readonly string ContainerPath = ConfigPaths.SpartaConfigResourcesPath + FolderName;
        static readonly string AssetFullPath = ContainerPath + FileName + FileExtension;
        #endif

        static IntroScreenData _instance;

        public static IntroScreenData Instance
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
        static IntroScreenData GetInstance()
        {
            if(_instance == null)
            {
                #if UNITY_EDITOR
                _instance = AssetDatabase.LoadAssetAtPath<IntroScreenData>(AssetFullPath);
                #else
                _instance = Resources.Load<IntroScreenData>(FolderName + FileName);
                #endif

                if(_instance == null)
                {
                    // If not found, autocreate the asset object.
                    _instance = CreateInstance<IntroScreenData>();

                    #if UNITY_EDITOR
                    CreateAsset();
                    #endif
                }
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
            }
        }

        #endif

        [SerializeField]
        Object _nextScene;

        public Object NextScene
        {
            get
            {
                return _nextScene;
            }
            private set
            {
                _nextScene = value;
            }
        }
    }
}
