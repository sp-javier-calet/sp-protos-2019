using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SocialPoint.Base;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SocialPoint.Dependency
{
    public static class InstallerAssetsManager
    {
        public static readonly string ContainerPath = ConfigPaths.SpartaConfigResourcesPath + "Installers";
        public const string FileExtension = ".asset";
        public const string AssetPattern = "*.asset";

        static string[] AssembliesToInspect = {
            "Assembly-CSharp",
            "Assembly-CSharp-firstpass"
        };

        [DidReloadScripts]
        static void OnScriptsReloaded()
        {
            Reload();
        }

        static public void Reload()
        {
            var installerType = typeof(Installer);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach(var assembly in assemblies)
            {
                bool valid = false;
                var assemblyName = assembly.GetName().Name;
                foreach(var validName in AssembliesToInspect)
                {
                    if(assemblyName == validName)
                    {
                        valid = true;
                        break;
                    }
                }

                if(!valid)
                {
                    continue;
                }

                foreach(var t in assembly.GetTypes())
                {
                    if(t.IsSubclassOf(installerType) && !t.IsAbstract)
                    {
                        CreateDefault(t);
                    }
                }
            }

            // Update GlobalDependencyConfigurer asset and save it

            var prefab = GlobalDependencyConfigurer.Load();
            prefab.Installers = Installers;
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();

            // Refresh all opened GlobalDependencyConfigurer windows

            foreach(var editor in Resources.FindObjectsOfTypeAll<GlobalDependencyConfigurerEditor>())
            {
                editor.Refresh();
            }
        }

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

        static string GetInstallerPath(MemberInfo t)
        {
            return GetInstallerPath(t.Name);
        }

        static string GetInstallerPath(MemberInfo t, string tag)
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