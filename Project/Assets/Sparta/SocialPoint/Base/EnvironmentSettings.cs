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

        const string FileName = "Environment";
        const string FileExtension = ".asset";
        const string ContainerPath = "Assets/Sparta/Config/Environment/Resources/";

        const string EnvironmentSettingsAssetPath = ContainerPath + FileName + FileExtension;

        static EnvironmentSettings _instance;

        public static EnvironmentSettings Instance
        {
            get
            {
                return GetInstance();
            }
        }

        #if UNITY_EDITOR
        static EnvironmentSettings()
        {
            #if !UNITY_5_4_OR_NEWER
            GetInstance();
            #endif
        }
        #else
        void OnEnable()
        {
            GetInstance();
        }
        #endif
        #if UNITY_5_4_OR_NEWER && UNITY_EDITOR
        [InitializeOnLoadMethod]
        #endif
        static EnvironmentSettings GetInstance()
        {
            if(_instance == null)
            {
                #if UNITY_EDITOR
                _instance = AssetDatabase.LoadAssetAtPath<EnvironmentSettings>(EnvironmentSettingsAssetPath);
                #else
                _instance = Resources.Load(FileName) as EnvironmentSettings;
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

            AssetDatabase.CreateAsset(Instance, EnvironmentSettingsAssetPath);
            AssetDatabase.SaveAssets();
        }

        static public void UpdateData()
        {
            UpdateEnvironmentUrl();
            UpdateAsset();
        }

        static void UpdateEnvironmentUrl()
        {
            Instance.EnvironmentUrl = string.Empty;
            EnvironmentSettings.Instance.EnvironmentUrl = System.Environment.GetEnvironmentVariable(EnvironmentUrlEnvironmentKey);
        }

        static void UpdateAsset()
        {
            EditorUtility.SetDirty(Instance);
        }

        #endif

        [SerializeField]
        string _environmentUrl = string.Empty;

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
