using System.IO;
using UnityEngine;

namespace SocialPoint.Base
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif

    public class EnvironmentSettings : ScriptableObject
    {
        const string EnvironmentUrlEnvironmentKey = "SP_ENVIRONMENT_URL";

        const string FileName = "Environment";
        const string ContainerPath = "Assets/Sparta/Config/Environment/Resources/";
        const string FileExtension = ".asset";

        const string EnvironmentSettingsAsset = ContainerPath + FileName + FileExtension;

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

        #if UNITY_EDITOR
        static EnvironmentSettings()
        {
            GetInstance();

            Instance.EnvironmentUrl = string.Empty;
            EnvironmentSettings.Instance.EnvironmentUrl = System.Environment.GetEnvironmentVariable(EnvironmentUrlEnvironmentKey);
        }
        #endif

        static EnvironmentSettings instance;

        public static EnvironmentSettings Instance
        {
            get
            {
                return GetInstance();
            }
        }

        static EnvironmentSettings GetInstance()
        {
            if(instance == null)
            {
                instance = Resources.Load(FileName) as EnvironmentSettings;
                if(instance == null)
                {
                    // If not found, autocreate the asset object.
                    instance = CreateInstance<EnvironmentSettings>();

                    #if UNITY_EDITOR
                    if(!Directory.Exists(ContainerPath))
                    {
                        Directory.CreateDirectory(ContainerPath);
                    }

                    string assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(EnvironmentSettingsAsset);
                    UnityEditor.AssetDatabase.CreateAsset(instance, assetPath);
                    UnityEditor.AssetDatabase.SaveAssets();
                    #endif
                }
            }
            return instance;

        }
    }
}
