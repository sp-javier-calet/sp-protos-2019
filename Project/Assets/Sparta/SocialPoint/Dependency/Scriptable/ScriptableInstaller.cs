using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace SocialPoint.Dependency
{
    public enum ModuleType
    {
        Configurer,
        Service,
        Game
    }

    public interface IScriptableInstaller
    {
        bool Enabled { get; }
        ModuleType Type { get; }
        void InstallBindings();
    }

    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif
    public abstract class ScriptableInstaller<T> : ScriptableObject, IScriptableInstaller where T : ScriptableObject
    {
        public bool Enabled { get; set; }
        public ModuleType Type { get; protected set; }


        #if UNITY_EDITOR
        static ScriptableInstaller()
        {
            Create();
        }
        #endif

        public const string ContainerPath = "Assets/Sparta/Config/Installers/";
        public const string FileExtension = ".asset";

        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();

        public static void Create()
        {
            if(!Directory.Exists(ContainerPath))
            {
                Directory.CreateDirectory(ContainerPath);
            }

            var path = GetInstallerPath();
            if(!File.Exists(path))
            {
                var asset = ScriptableObject.CreateInstance<T>();
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
            }
        }

        public static string GetInstallerPath()
        {
            return ContainerPath + typeof(T).Name + FileExtension;
        }

        public static bool Delete()
        {
            return AssetDatabase.DeleteAsset(GetInstallerPath());
        }
    }

    public class AdminPanelScriptInstaller : ScriptableInstaller<AdminPanelScriptInstaller>
    {
        public bool ShowButton;

        public AdminPanelScriptInstaller()
        {
            Type = ModuleType.Service;
        }

        public override void InstallBindings()
        {

        }
    }

    public class HttpClientScriptInstaller : ScriptableInstaller<HttpClientScriptInstaller>
    {
        public string Config = "basegame";
        public bool EnableHttpStreamPinning = false;

        public HttpClientScriptInstaller()
        {
            Type = ModuleType.Service;
        }

        public override void InstallBindings()
        {

        }

        public new static void Create()
        {
            ScriptableInstaller<HttpClientScriptInstaller>.Create();
        }
    }

    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    #endif
    public class SocialFrameworkInstaller : ScriptableInstaller<SocialFrameworkInstaller>
    {
        const string DefaultWAMPProtocol = "wamp.2.json";
        const string DefaultEndpoint = "ws://sprocket-00.int.lod.laicosp.net:8001/ws";

        [Serializable]
        public class SettingsData
        {
            public string Endpoint = DefaultEndpoint;
            public string[] Protocols = new string[] { DefaultWAMPProtocol };
        }

        public SettingsData Settings = new SettingsData();

        public SocialFrameworkInstaller()
        {
            Type = ModuleType.Service;
        }

        public override void InstallBindings()
        {

        }

        #if UNITY_EDITOR
        static SocialFrameworkInstaller()
        {
            Create();
        }
        #endif
    }
}
