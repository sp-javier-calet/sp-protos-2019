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
        static private string GetCommandLineArg(string name, string def)
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
        
        [PostProcessBuild(101)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if(target == BuildTarget.iOS || target == BuildTarget.tvOS)
            {
                Debug.Log("executing SocialPoint DependencyManager PostProcessor on path '" + path + "'...");
                
                XCProject project = new XCProject(path);

                var spxcodemods = new List<string>(Directory.GetFiles(Application.dataPath, "base.*.spxcodemod", SearchOption.AllDirectories));
                if(Scheme != null)
                {
                    spxcodemods.AddRange(Directory.GetFiles(Application.dataPath, Scheme + ".*.spxcodemod", SearchOption.AllDirectories));
                }
                foreach(string file in spxcodemods)
                {
                    Debug.Log(string.Format("applying '{0}'...", file));
                    project.ApplyMod(file);
                }

                if(Build != null)
                {
                    Debug.Log(string.Format("setting build '{0}'...", Build));
                    Hashtable table = new Hashtable();
                    table["CFBundleVersion"] = Build;
                    project.CombineInfoPlist(table);
                }

                if(Version != null)
                {
                    Debug.Log(string.Format("setting version '{0}'...", Version));
                    Hashtable table = new Hashtable();
                    table["CFBundleShortVersionString"] = Version;
                    project.CombineInfoPlist(table);
                }
                
                project.Save();
            }
        }
    }
}
