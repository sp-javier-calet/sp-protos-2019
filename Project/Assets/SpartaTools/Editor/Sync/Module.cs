using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SpartaTools.Editor.Sync
{
    public class Module
    {
        public const string DefinitionFileName = ".sparta_module";

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
                return;

            if(parts[0] == "type")
            {
                switch(parts[1])
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
            else if(parts[0] == "name")
            {
                Name = parts[1];
            }
            else if(parts[0] == "depends_on")
            {
                Dependencies.Add(parts[1]);
            }
            else if(parts[0] == "desc")
            {
                Description = parts[1];
            }
        }
    }
}