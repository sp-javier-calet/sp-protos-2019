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

        public static string SourcesDirAbsolute;
        public static string BinariesDirAbsolute;
        public static string CoreDirAbsolute;
        public static string ExternalDirAbsolute;
        public static string ExtensionsDirAbsolute;

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

            SourcesDirAbsolute = Directory.GetDirectories(_currentDir, _sources, SearchOption.AllDirectories)[0];
            BinariesDirAbsolute = GetAbsoluteSpartaPath(_binaries);
            CoreDirAbsolute = GetAbsoluteSpartaPath(_core);
            ExternalDirAbsolute = GetAbsoluteSpartaPath(_external);
            ExtensionsDirAbsolute = GetAbsoluteSpartaPath(_extensions);

            SourcesDir = TransformToRelativePath(SourcesDirAbsolute);
            BinariesDir = TransformToRelativePath(BinariesDirAbsolute);
            CoreDir = TransformToRelativePath(CoreDirAbsolute);
            ExternalDir = TransformToRelativePath(ExternalDirAbsolute);
            ExtensionsDir = TransformToRelativePath(ExtensionsDirAbsolute);
        }

        static string GetAbsoluteSpartaPath(string pattern)
        {
            foreach(var dir in _spartaDirs)
            {
                var subDirs = Directory.GetDirectories(dir, pattern, SearchOption.AllDirectories);
                if(subDirs.Length > 0)
                {
                    return subDirs[0];
                }
            }
            return string.Empty;
        }

        static string TransformToRelativePath(string absolutePath)
        {
            return absolutePath.Replace(_currentDir + Path.DirectorySeparatorChar, string.Empty);
        }

        public static string ReplaceProjectVariables(string originalPath, IDictionary<string, string> projectVariables)
        {
            var path = originalPath;

            foreach(var entry in projectVariables)
            {
                var pattern = string.Format("{{{0}}}", entry.Key);
                path = path.Replace(pattern, entry.Value);
            }

            return path;
        }
    }
}
