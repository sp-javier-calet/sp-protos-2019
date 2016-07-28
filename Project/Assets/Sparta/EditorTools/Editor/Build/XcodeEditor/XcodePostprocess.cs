using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    public static class XcodePostprocess
    {
        public const string BaseScheme = "base";
        public const string EditorScheme = "editor";

        static string[] Schemes
        {
            get
            {
                // XCodeModSchemes prefs are written by BuildSet.
                var customPrefixes = EditorPrefs.GetString(BuildSet.XcodeModSchemesPrefsKey, string.Empty);
                if(string.IsNullOrEmpty(customPrefixes))
                {
                    return new string[0];
                }
                else
                {
                    return customPrefixes.Split(new char[]{ ';' });
                }
            }
        }

        static void Log(string message)
        {
            Debug.Log(string.Format("XcodeMods Editor: {0}", message));
        }


        public static string GetProjectPath(string basePath)
        {
            var projectPath = basePath;
            if(!basePath.EndsWith(".xcodeproj"))
            {
                string[] projects = Directory.GetDirectories(basePath, "*.xcodeproj");
                if(projects.Length > 0)
                {
                    projectPath = projects[0];
                }
            }

            return projectPath;
        }

        [PostProcessBuild(701)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if(target == BuildTarget.iOS || target == BuildTarget.tvOS)
            {
                Log("Executing SocialPoint xcodemods PostProcessor on path '" + path + "'...");

                var projectPath = GetProjectPath(path);
                if(!Directory.Exists(projectPath))
                {
                    throw new FileNotFoundException(string.Format("Xcode project filed not found in path '{0}'", projectPath));
                }

                var baseAppPath = Path.Combine(UnityEngine.Application.dataPath, "..");

                var project = new XcodeProject(projectPath, baseAppPath);
                var mods = new XcodeModsSet(target);

                Log("Enabling 'base' scheme for xcodemods");
                mods.AddScheme(BaseScheme);

                var schemes = Schemes;
                Log(string.Format("Enabling config schemes for xcodemods: {0}", string.Join(", ", schemes)));
                mods.AddScheme(schemes);

                if(UnityEditorInternal.InternalEditorUtility.isHumanControllingUs &&
                    !UnityEditorInternal.InternalEditorUtility.inBatchMode)
                {
                    mods.AddScheme(EditorScheme);
                    Log("Enabling 'editor' scheme for xcodemods");
                }

                foreach(string file in mods.Files)
                {
                    Log(string.Format("Applying file '{0}'", Path.GetFileName(file)));
                    var mod = new XcodeMod(file);
                    mod.Apply(project.Editor);
                }

                project.Editor.Commit();
            }
        }
    }
}

