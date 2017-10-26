using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    public static class XcodePostprocess
    {
        public const string BaseScheme = "base";
        public const string EditorScheme = "editor";
        public const string AppendScheme = "append";
        public const string JenkinsScheme = "jenkins";

        const string LastPathPrefsKey = "XcodePostProcessLastPath";

        public static string LastProjectPath
        {
            set
            {
                EditorPrefs.SetString(LastPathPrefsKey, value);
            }
            get
            {
                return EditorPrefs.GetString(LastPathPrefsKey, string.Empty);
            }
        }

        static string[] Schemes
        {
            get
            {
                // XCodeModSchemes prefs are written by BuildSet.
                var customPrefixes = BuildSet.CurrentXcodeModSchemes;
                return string.IsNullOrEmpty(customPrefixes) ? new string[0] : customPrefixes.Split(new [] { ';' });
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

            return Path.GetFullPath(projectPath);
        }


        // check for in-editor builds (replace button removes folder content first, append does not)
        static bool CheckIfManualAppend(string path)
        {
            string appendCheckFilePath = Path.Combine(path, "appendcheck.txt");

            if(File.Exists(appendCheckFilePath))
            {
                return true;
            }
            File.Create(appendCheckFilePath).Close();
            return false;
        }

        static bool IsAppendingBuild(string path)
        {
            if(AutoBuilder.IsRunning)
            {
                if(AutoBuilder.IsAppendingBuild)
                {
                    Log("(Autobuilder Build) XcodeMods will not be applied due to appending build, except the ones with 'append' scheme");
                    return true;
                }
            }
            else if(CheckIfManualAppend(path)) // check only for manual builds
            {
                Log("(Manual Build) XcodeMods will not be applied due to appending build, except the ones with 'append' scheme");
                return true;
            }

            return false;
        }


        [PostProcessBuild(701)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            ApplyXcodeMods(target, path);
        }

        public static void ApplyXcodeMods(BuildTarget target, string path)
        {
            if(target == BuildTarget.iOS || target == BuildTarget.tvOS)
            {
                Log("Executing SocialPoint xcodemods PostProcessor on path '" + path + "'...");

                // Store project path for manual execution
                LastProjectPath = path;

                var baseAppPath = Path.Combine(Application.dataPath, "..");

                var projectPath = GetProjectPath(path);

                // Manage relative paths if needed
                if(!Path.IsPathRooted(projectPath))
                {
                    projectPath = Path.Combine(baseAppPath, projectPath);
                }

                if(!Directory.Exists(projectPath))
                {
                    throw new FileNotFoundException(string.Format("Xcode project filed not found in path '{0}'", projectPath));
                }

                var project = new XcodeProject(projectPath, baseAppPath);
                var mods = new XcodeModsSet(target);

                if(IsAppendingBuild(path))
                {
                    mods.AddScheme(AppendScheme);
                    Log("Enabling 'append' scheme for xcodemods");
                }
                else
                {
                    Log("Enabling 'base' scheme for xcodemods");
                    mods.AddScheme(BaseScheme);

                    var schemes = Schemes;
                    Log(string.Format("Enabling config schemes for xcodemods: {0}", string.Join(", ", schemes)));
                    mods.AddScheme(schemes);

                    if(IsEditorMode)
                    {
                        mods.AddScheme(EditorScheme);
                        Log("Enabling 'editor' scheme for xcodemods");
                    }

                    if(IsJenkinsMode)
                    {
                        mods.AddScheme(JenkinsScheme);
                        Log("Enabling 'jenkins' scheme for xcodemods");
                    }
                }

                foreach(string file in mods.Files)
                {
                    Log(string.Format("Applying file '{0}'", Path.GetFileName(file)));
                    var mod = new XcodeMod(file);
                    mod.Apply(project.Editor);
                }

                try
                {
                    project.Editor.Commit();
                }
                catch(Exception e)
                {
                    #if UNITY_EDITOR
                    if(IsEditorMode)
                    {
                        EditorUtility.DisplayDialog("Xcode Postprocess Error", "There was an error while applying XcodeMods. XcodeProject probably has an invalid state. Error: " + e.Message, "I will close Xcode"); 
                    }
                    #endif

                    throw e;
                }
            }
        }

        static bool IsEditorMode
        {
            get
            {
                #if UNITY_EDITOR
                return UnityEditorInternal.InternalEditorUtility.isHumanControllingUs &&
                !UnityEditorInternal.InternalEditorUtility.inBatchMode;
                #else
                return false;
                #endif
            }
        }

        static bool IsJenkinsMode
        {
            get
            {
                return !UnityEditorInternal.InternalEditorUtility.isHumanControllingUs &&
                UnityEditorInternal.InternalEditorUtility.inBatchMode;
            }
        }
    }
}

