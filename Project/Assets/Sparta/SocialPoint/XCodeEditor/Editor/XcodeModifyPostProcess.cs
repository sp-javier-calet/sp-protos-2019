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
        static string GetCommandLineArg(string name, string def)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            name = "+" + name + "=";
            foreach(string arg in arguments)
            {
                if(arg.StartsWith(name))
                {
                    return arg.Substring(name.Length);
                }
            }
            return def;
        }
        
        static string Build
        {   
            get
            {
                return GetCommandLineArg("build", DateTime.Now.ToString("yyMMddHHmm"));
            }
        }
        
        static string Scheme
        {   
            get
            {
                return GetCommandLineArg("scheme", "editor").ToLower();
            }

        }
        
        static string Version
        {   
            get
            {
                return GetCommandLineArg("version", null);
            }

        }
        
        [PostProcessBuild(701)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if(target == BuildTarget.iOS || target == BuildTarget.tvOS)
            {
                var spxcodemods = new List<string>();

                var patterns = new List<string>();
                var patternsScheme = new List<string>();
                patterns.Add("base.*.spxcodemod");
                patternsScheme.Add(".*.spxcodemod");

                if(target == BuildTarget.iOS)
                {
                    patterns.Add("ios.base.*.spxcodemod");
                    patternsScheme.Add("ios.*.spxcodemod");
                }
                if(target == BuildTarget.tvOS)
                {
                    patterns.Add("tvos.base.*.spxcodemod");
                    patternsScheme.Add("tvos.*.spxcodemod");
                }

                Debug.Log("executing SocialPoint DependencyManager PostProcessor on path '" + path + "'...");
                
                var project = new XCProject(path);

                foreach(var pattern in patterns)
                {
                    spxcodemods.AddRange(Directory.GetFiles(Application.dataPath, pattern, SearchOption.AllDirectories));
                }

                if(Scheme != null)
                {
                    foreach(var pattern in patternsScheme)
                    {
                        spxcodemods.AddRange(Directory.GetFiles(Application.dataPath, Scheme + pattern, SearchOption.AllDirectories));
                    }
                }
                foreach(string file in spxcodemods)
                {
                    Debug.Log(string.Format("applying '{0}'...", file));
                    project.ApplyMod(file);
                }

                if(Build != null)
                {
                    Debug.Log(string.Format("setting build '{0}'...", Build));
                    var table = new Hashtable();
                    table["CFBundleVersion"] = Build;
                    project.CombineInfoPlist(table);
                }

                if(Version != null)
                {
                    Debug.Log(string.Format("setting version '{0}'...", Version));
                    var table = new Hashtable();
                    table["CFBundleShortVersionString"] = Version;
                    project.CombineInfoPlist(table);
                }
                
                project.Save();
            }
        }
    }
}
