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

    public class EnvironmentSettings : ScriptableObject
    {
        const string EnvironmentUrlEnvironmentKey = "SP_DEFAULT_ENVIRONMENT";

        const string FolderName = "Environment/";
        const string FileName = "Environment";

        #if UNITY_EDITOR
        const string FileExtension = ".asset";
        static readonly string ContainerPath = ConfigPaths.SpartaConfigResourcesPath + FolderName;
        static readonly string AssetFullPath = ContainerPath + FileName + FileExtension;
        #endif

        static EnvironmentSettings _instance;

        public static EnvironmentSettings Instance
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
        static EnvironmentSettings GetInstance()
        {
            if(_instance == null)
            {
                #if UNITY_EDITOR
                _instance = AssetDatabase.LoadAssetAtPath<EnvironmentSettings>(AssetFullPath);
                #else
                _instance = Resources.Load<EnvironmentSettings>(FolderName + FileName);
                #endif

                if(_instance == null)
                {
                    // If not found, autocreate the asset object.
                    _instance = CreateInstance<EnvironmentSettings>();

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
            }
        }

        static public void UpdateData()
        {
            UpdateEnvironmentUrl();
            UpdateAsset();
        }

        static void UpdateEnvironmentUrl()
        {
            _instance.EnvironmentUrl = string.Empty;
            EnvironmentSettings._instance.EnvironmentUrl = System.Environment.GetEnvironmentVariable(EnvironmentUrlEnvironmentKey);
        }

        static void UpdateAsset()
        {
            EditorUtility.SetDirty(_instance);
        }

        #endif

        [SerializeField]
        string _environmentUrl;

        public string EnvironmentUrl
        {
            get
            {
                return _environmentUrl ?? string.Empty;
            }
            private set
            {
                _environmentUrl = value;
            }
        }
    }
}
