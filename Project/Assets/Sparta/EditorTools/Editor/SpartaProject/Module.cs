﻿using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SpartaTools.Editor.SpartaProject
{
    public class Module
    {
        public const string DefinitionFileName = ".sparta_module";

        static readonly IModuleFilter DefaultDSFilter = new ExtensionFilter(".DS_Store");

        public enum ModuleType
        {
            Core,
            Sources,
            Binaries,
            Extension,
            External
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public ModuleType Type { get; private set; }

        public IList<string> Dependencies { get; private set; }

        public IList<IModuleFilter> Filters { get; private set; }

        public bool Valid { get; set; }

        public string ModuleFile { get; private set; }

        public string RelativePath { get; private set; }

        public bool IsMandatory
        {
            get
            {
                return Type == ModuleType.Core || Type == ModuleType.Sources || Type == ModuleType.Binaries;
            }
        }

        public Module(string projectPath, string moduleFile)
        {
            Description = "No description";

            var folderPath = Path.GetDirectoryName(moduleFile);
            var fullPath = new Uri(folderPath, UriKind.Absolute);
            var relRoot = new Uri(projectPath + "/", UriKind.Absolute);

            RelativePath = relRoot.MakeRelativeUri(fullPath).ToString();
            ModuleFile = moduleFile;
            Name = RelativePath;

            Type = ModuleType.Extension;
            Dependencies = new List<string>();

            Filters = new List<IModuleFilter>();
            Filters.Add(DefaultDSFilter);

            Valid = Parse(moduleFile);
        }

        bool Parse(string filePath)
        {
            if(!File.Exists(filePath))
            {
                return false;
            }

            var file = new StreamReader(filePath);
            var line = file.ReadLine();
            while(line != null)
            {
                ParseLine(line);
                line = file.ReadLine();
            }
            file.Close();
            return true;
        }

        void ParseLine(string line)
        {
            string[] parts = line.Split(null, 2);
            if(parts.Length != 2)
            {
                Debug.LogWarningFormat("Skipping parsing for module definition line: {0}", line);
                return;
            }

            var tag = parts[0];
            var content = parts[1];

            if(tag == "type")
            {
                switch(content)
                {
                case "core":
                    Type = ModuleType.Core;
                    break;

                case "sources":
                    Type = ModuleType.Sources;
                    break;
					
                case "binaries":
                    Type = ModuleType.Binaries;
                    break;

                case "extension":
                    Type = ModuleType.Extension;
                    break;

                case "external":
                    Type = ModuleType.External;
                    break;
                }
            }
            else if(tag == "name")
            {
                Name = content;
            }
            else if(tag == "depends_on")
            {
                Dependencies.Add(content);
            }
            else if(tag == "desc")
            {
                Description = content;
            }
            else if(tag == "exclude_path")
            {
                Filters.Add(new PathFilter(content));
            }
            else if(tag == "exclude_extension")
            {
                Filters.Add(new ExtensionFilter(content));
            }
            else
            {
                Debug.LogWarningFormat("Unknown Sparta module option '{0}'", tag);
            }
        }

        #region Module filtering

        public interface IModuleFilter
        {
            bool Apply(FileInfo info, string relativePath);
        }

        class PathFilter : IModuleFilter
        {
            string Path;
            string PathMeta;

            public PathFilter(string path)
            {
                Path = path;
                PathMeta = path + ".meta";
            }

            public bool Apply(FileInfo inf, string relativePath)
            {
                return Path == relativePath || PathMeta == relativePath;
            }
        }

        class ExtensionFilter : IModuleFilter
        {
            string Extension;

            public ExtensionFilter(string extension)
            {
                Extension = extension;
            }

            public bool Apply(FileInfo info, string relativePath)
            {
                return info.Extension.Equals(Extension);
            }
        }

        #endregion
    }
}