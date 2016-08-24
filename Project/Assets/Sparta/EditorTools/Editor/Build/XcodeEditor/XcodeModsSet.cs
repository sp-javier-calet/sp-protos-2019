using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Discovers and manage a set of Xcodemods files.
    /// Uses the Target Platform and Schemes to match spxcodemod files within the project.
    /// 
    /// XcodeMod files must have the following structure: 
    ///     [platform.]scheme.module.spxcodemod
    /// where:
    /// 'platform' is an optional prefix, which is infered from build settings during compilation ('ios', 'tvos'...)
    /// 'scheme' is the mod scheme. 'base' mods are always applied, and 'editor' is automatically applied when launched from editor.
    ///          Other schemes include 'debug', 'release', 'shipping', or any other custom scheme.
    /// 'module' is the module name.
    /// 
    /// </summary>
    public class XcodeModsSet
    {
        const string XcodeModPattern = ".*.spxcodemod";

        readonly string _platformPrefix;
        readonly List<string> _patterns = new List<string>();

        public XcodeModsSet(BuildTarget target)
        {
            _platformPrefix = target.ToString().ToLower() + ".";
        }

        public void AddScheme(string scheme)
        {
            _patterns.Add(scheme + XcodeModPattern);
            _patterns.Add(_platformPrefix + scheme + XcodeModPattern);
        }

        public void AddScheme(string[] schemes)
        {
            foreach(var scheme in schemes)
            {
                AddScheme(scheme);
            }
        }

        public List<string> Files
        {
            get
            {
                var spxcodemods = new List<string>();
                foreach(var pattern in _patterns)
                {
                    var files = Directory.GetFiles(Application.dataPath, pattern, SearchOption.AllDirectories);
                    spxcodemods.AddRange(files);
                }

                return spxcodemods;
            }
        }
    }
}
