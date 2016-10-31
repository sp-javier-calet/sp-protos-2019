using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
        bool Enabled { get; set; }
        ModuleType Type { get; }
        void InstallBindings();
    }

    public static class ScriptableInstallerManager
    {
        public const string ContainerPath = "Assets/Sparta/Config/Installers";
        public const string FileExtension = ".asset";

        public static void Create(Type t)
        {
            if(!Directory.Exists(ContainerPath))
            {
                Directory.CreateDirectory(ContainerPath);
            }

            var path = GetInstallerPath(t);
            if(!File.Exists(path))
            {
                var asset = ScriptableObject.CreateInstance(t);
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
            }
        }

        public static ScriptableInstaller Open(string path)
        {
            return AssetDatabase.LoadAssetAtPath<ScriptableInstaller>(path);
        }

        public static string GetInstallerPath(Type t)
        {
            return ContainerPath + "/" + t.Name + FileExtension;
        }

        public static bool Delete(Type t)
        {
            return AssetDatabase.DeleteAsset(GetInstallerPath(t));
        }

        public static ScriptableInstaller[] Installers
        {
            get
            {
                var list = new List<ScriptableInstaller>();
                var files = Directory.GetFiles(ContainerPath, "*.asset");
                foreach(var f in files)
                {
                    var asset = Open(f);
                    if(asset != null && asset.Type != ModuleType.Configurer)
                    {
                        list.Add(asset);
                    }
                }

                return list.ToArray();
            }
        }
    }

    public abstract class ScriptableInstaller : ScriptableObject, IScriptableInstaller
    {
        public bool Enabled { get; set; }
        public ModuleType Type { get; private set; }

        protected ScriptableInstaller()
        {
            Type = ModuleType.Service;
        }

        protected ScriptableInstaller(ModuleType type)
        {
            Type = type;
        }

        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }
}
