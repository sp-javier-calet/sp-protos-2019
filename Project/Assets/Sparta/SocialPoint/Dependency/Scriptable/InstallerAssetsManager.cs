using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.Dependency
{
    public static class InstallerAssetsManager
    {
        public const string ContainerPath = "Assets/Sparta/Config/Resources/Installers";
        public const string FileExtension = ".asset";
        public const string AssetPattern = "*.asset";

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
                var files = Directory.GetFiles(ContainerPath, AssetPattern);
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

        public static ScriptableInstaller[] Configurers
        {
            get
            {
                var list = new List<ScriptableInstaller>();
                var files = Directory.GetFiles(ContainerPath, AssetPattern);
                foreach(var f in files)
                {
                    var asset = Open(f);
                    if(asset != null && asset.Type == ModuleType.Configurer)
                    {
                        list.Add(asset);
                    }
                }

                return list.ToArray();
            }
        }
    }
}