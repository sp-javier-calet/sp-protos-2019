using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.XCodeEditor
{
    public static class XcodeModifyPostProcess
    {
        static string[] Schemes
        {
            get
            {
                var customPrefixes = EditorPrefs.GetString("XCodeModSchemes", string.Empty);
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

        [PostProcessBuild(701)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if(target == BuildTarget.iOS || target == BuildTarget.tvOS)
            {
                XcodeMods.Apply(target, path, Schemes);
            }
        }
    }

    public static class XcodeMods
    {
        public const string BaseScheme = "base";
        public const string EditorScheme = "editor";

        class XcodeModsSet
        {
            const string XcodeModPattern = ".*.spxcodemod";

            readonly string _platformPrefix;
            readonly List<string> _patterns = new List<string>();

            public XcodeModsSet(BuildTarget target)
            {
                _platformPrefix = target.ToString().ToLower() + ".";
            }

            public void Add(string scheme)
            {
                _patterns.Add(scheme + XcodeModPattern);
                _patterns.Add(_platformPrefix + scheme + XcodeModPattern);
            }

            public void Add(string[] schemes)
            {
                foreach(var scheme in schemes)
                {
                    Add(scheme);
                }
            }

            public List<string> Files
            {
                get
                {
                    var spxcodemods = new List<string>();
                    foreach(var pattern in _patterns)
                    {
                        spxcodemods.AddRange(Directory.GetFiles(Application.dataPath, pattern, SearchOption.AllDirectories));
                    }

                    return spxcodemods;
                }
            }
        }

        public static void Apply(BuildTarget target, string path, string[] schemes)
        {
            Debug.Log("Executing SocialPoint DependencyManager PostProcessor on path '" + path + "'...");

            var project = new XCProject(path);
            var mods = new XcodeModsSet(target);

            mods.Add(BaseScheme);
            mods.Add(schemes);

            if(UnityEditorInternal.InternalEditorUtility.isHumanControllingUs &&
               !UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                mods.Add(EditorScheme);
            }

            // Apply Mods
            foreach(string file in mods.Files)
            {
                Debug.Log(string.Format("Applying '{0}'...", file));
                project.ApplyMod(file);
            }

            project.Save();
        }
    }
}
