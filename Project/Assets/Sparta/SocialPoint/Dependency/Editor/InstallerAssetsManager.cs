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

        public static bool CreateDefault(Type t)
        {
            return Create(t, string.Empty, false);
        }

        public static bool Create(Type t, string tag, bool focus)
        {
            if(!typeof(Installer).IsAssignableFrom(t))
            {
                throw new InvalidCastException(string.Format("Invalid type '{0}' when creating an Installer instance", t.Name));
            }

            if(!Directory.Exists(ContainerPath))
            {
                Directory.CreateDirectory(ContainerPath);
            }

            var path = GetInstallerPath(t, tag);
            if(!File.Exists(path))
            {
                var asset = (Installer)ScriptableObject.CreateInstance(t);
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();

                if(focus)
                {
                    ProjectWindowUtil.ShowCreatedAsset(asset);
                    EditorUtility.FocusProjectWindow();
                }
                return true;
            }

            return false;
        }

        static Installer Open(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Installer>(path);
        }

        static string GetInstallerPath(Type t)
        {
            return GetInstallerPath(t.Name);
        }

        static string GetInstallerPath(Type t, string tag)
        {
            var name = t.Name;
            if(!string.IsNullOrEmpty(tag))
            {
                name = string.Format("{0} - {1}", name, tag);
            }
            return GetInstallerPath(name);
        }

        static string GetInstallerPath(string name)
        {
            return ContainerPath + "/" + name + FileExtension;
        }

        public static bool Duplicate(Installer installer)
        {
            return Create(installer.GetType(), "Copy", true);
        }

        public static bool Delete(Installer installer)
        {
            var deleted = AssetDatabase.DeleteAsset(GetInstallerPath(installer.name));
            if(deleted)
            {
                AssetDatabase.SaveAssets();
            }

            return deleted;
        }

        public static Installer[] Installers
        {
            get
            {
                var list = new List<Installer>();
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

        public static Installer[] Configurers
        {
            get
            {
                var list = new List<Installer>();
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