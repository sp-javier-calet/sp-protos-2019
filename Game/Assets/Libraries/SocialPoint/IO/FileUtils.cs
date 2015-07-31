using UnityEngine;
using System.Collections;
using System.IO;

namespace SocialPoint.IO {

    public class FileUtils 
    {
        private static WWW Download(string path)
        {
            var www = new WWW(path);
            while (!www.isDone);
            return www;
        }
        
        public static bool IsWritable(string path)
        {
            if(IsUrl(path))
            {
                return false;
            }
            else
            {
                var info = new System.IO.FileInfo(path);
                return !info.Exists || !info.IsReadOnly; // FIXME IF file doesnt exist, check folder permissions
            }
        }
        
        public static void Copy(string from, string to, bool overwrite = false)
        {
            if (Exists (to)) 
            {
                if (!overwrite) 
                {
                    throw new IOException ("Destination exists.");
                }
            }
            
            CheckWritablePath(to);
            
            var bytes = ReadAllBytes(from);
            WriteAllBytes(to, bytes);
        }
        
        public static bool Exists(string path)
        {
            if (IsUrl(path))
            {
                var www = Download(path);
                return string.IsNullOrEmpty(www.error);
            }
            else
            {
                return System.IO.File.Exists(path);
            }
        }
        
        public static bool IsUrl(string path)
        {
            return path.Contains("://");
        }
        
        public static string ReadAllText(string path)
        {
            if (IsUrl(path))
            {
                var www = Download(path);
                return www.text;
            }
            else
            {
                return System.IO.File.ReadAllText(path);
            }
        }
        
        public static byte[] ReadAllBytes(string path)
        {
            if (IsUrl(path))
            {
                var www = Download(path);
                return www.bytes;
            }
            else
            {
                return System.IO.File.ReadAllBytes(path);
            }
        }
        
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            CheckWritablePath(path);
            CreateFile(path, true);
            File.WriteAllBytes(path, bytes);
        }
        
        public static void WriteAllText(string path, string text)
        {
            CheckWritablePath(path);
            CreateFile(path, true);
            File.WriteAllText(path, text);
        }

        public static string [] GetFilesInDirectory(string path)
        {
            return Directory.GetFiles(path);
        }

        public static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static void CreateFile(string path, bool overwrite = false)
        {
            if(Exists(path) && !overwrite) 
            {
                throw new IOException ("File already exists.");
            }

            string dirPath = Path.GetDirectoryName(path);
            if(!Exists(dirPath))
            {
                CreateDirectory(dirPath);
            }
            File.Create(path).Close();
        }
        
        public static void Delete(string path)
        {
            if(!Exists(path))
            {
                return;
            }

            FileAttributes attributes = File.GetAttributes(path);
            if((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(path);
            }
            else
            {
                File.Delete(path);
            }
        }

        private static void CheckWritablePath(string path)
        {
            if(!IsWritable(path))
            {
                throw new IOException("Destination needs to be writable.");
            }
        }
    }
}