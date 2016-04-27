using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AssetSerializer.Helpers
{
    public class FileHelper
    {
        public static List<FileInfo> GetFilesFromPathWithSearchPattern (string path, string searchPattern, bool recursive = false)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            List<FileInfo> files = new List<FileInfo>();

            GetFilesFromDirectoryWithSearchPattern(ref files, dir, searchPattern, recursive);

            return files;
        }

        private static void GetFilesFromDirectoryWithSearchPattern (ref List<FileInfo> files, DirectoryInfo dir, string searchPattern, bool recursive = false)
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

        public static string GetFileStringContent (string filePath)
        {
            string data = "";

            using(FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {                    
                using(StreamReader sr = new StreamReader(fs))
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