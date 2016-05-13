using System.Collections.Generic;
using System.IO;

namespace SocialPoint.AssetSerializer.Helpers
{
    public static class FileHelper
    {
        public static List<FileInfo> GetFilesFromPathWithSearchPattern(string path, string searchPattern, bool recursive = false)
        {
            var dir = new DirectoryInfo(path);

            var files = new List<FileInfo>();

            GetFilesFromDirectoryWithSearchPattern(ref files, dir, searchPattern, recursive);

            return files;
        }

        static void GetFilesFromDirectoryWithSearchPattern(ref List<FileInfo> files, DirectoryInfo dir, string searchPattern, bool recursive = false)
        {
            FileInfo[] items = dir.GetFiles(searchPattern);
            foreach(FileInfo info in items)
            {
                files.Add(info);
            }

            if(recursive)
            {
                DirectoryInfo[] directories = dir.GetDirectories();
                foreach(DirectoryInfo tmpDir in directories)
                {
                    GetFilesFromDirectoryWithSearchPattern(ref files, tmpDir, searchPattern, recursive);
                }
            }
        }

        public static string GetFileStringContent(string filePath)
        {
            string data = "";

            using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {                    
                using(var sr = new StreamReader(fs))
                {
                    while(!sr.EndOfStream)
                    {
                        data += sr.ReadLine();
                    }
                }
            }

            return data;
        }
    }
}