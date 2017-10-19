using System;
using System.Collections.Generic;
using System.IO;

namespace SpartaTools.Editor.Utils
{
    public static class SpartaPaths
    {
        public const string SourcesVariable = "SPARTA_SOURCES_PATH";
        public const string BinariesVariable = "SPARTA_BINARIES_PATH";
        public const string CoreVariable = "SPARTA_CORE_PATH";
        public const string ExternalVariable = "SPARTA_EXTERNAL_PATH";
        public const string ExtensionsVariable = "SPARTA_EXTENSIONS_PATH";

        public static string SourcesDir;
        public static string BinariesDir;
        public static string CoreDir;
        public static string ExternalDir;
        public static string ExtensionsDir;

        const string _sparta = "Sparta";
        const string _sources = "Sources";
        const string _binaries = "Binaries";
        const string _core = "SocialPoint";
        const string _external = "External";
        const string _extensions = "Extensions";

        static string _currentDir;
        static readonly string[] _spartaDirs;

        static SpartaPaths()
        {
            _currentDir = Directory.GetCurrentDirectory();

            _spartaDirs = Directory.GetDirectories(_currentDir, _sparta, SearchOption.AllDirectories);

            SourcesDir = Directory.GetDirectories(_currentDir, _sources, SearchOption.AllDirectories)[0];
            SourcesDir = TransformToRelativePath(SourcesDir);

            BinariesDir = GetRelativeSpartaPath(_binaries);
            CoreDir = GetRelativeSpartaPath(_core);
            ExternalDir = GetRelativeSpartaPath(_external);
            ExtensionsDir = GetRelativeSpartaPath(_extensions);
        }

        static string GetRelativeSpartaPath(string pattern)
        {
            foreach(var dir in _spartaDirs)
            {
                var subDirs = Directory.GetDirectories(dir, pattern, SearchOption.AllDirectories);
                if(subDirs.Length > 0)
                {
                    return TransformToRelativePath(subDirs[0]);
                }
            }
            return string.Empty;
        }

        static string TransformToRelativePath(string absolutePath)
        {
            return absolutePath.Replace(_currentDir, string.Empty);
        }

        public static string ReplaceProjectVariables(string basePath, string originalPath, IDictionary<string, string> projectVariables)
        {
            var path = originalPath;

            foreach(var entry in projectVariables)
            {
                var pattern = string.Format("{{{0}}}", entry.Key);
                path = path.Replace(pattern, entry.Value);
            }

            // If is not already a full path, use the base path if possible
            if(!Path.IsPathRooted(path) && !string.IsNullOrEmpty(basePath))
            {
                path = Path.Combine(basePath, path);
            }

            return path;
        }
    }
}
