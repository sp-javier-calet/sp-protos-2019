#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
#define UNITY
#endif


#if UNITY
using UnityEngine;
#endif

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SocialPoint.IO {

    public class FileUtils 
    {

#if UNITY
        private static WWW Download(string path)
        {
            var www = new WWW(path);
            while (!www.isDone);
            return www;
        }
#endif

        private const char WildcardMultiChar = '*';
        private const char WildcardOneChar = '?';

        public delegate bool OperationFilter(string src, string dst);
        
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

        public static string Combine(string basePath, string relPath)
        {
            if(IsUrl(basePath))
            {
                return new Uri(new Uri(basePath), relPath).AbsoluteUri;
            }
            else
            {
                return Path.Combine(basePath, relPath);
            }
        }
        
        public static void CopyFile(string from, string to, bool overwrite = false)
        {
            if(Exists(to)) 
            {
                if (!overwrite) 
                {
                    throw new IOException("Destination exists.");
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
#if UNITY
                var www = Download(path);
                return string.IsNullOrEmpty(www.error);
#else
                throw new IOException("Url paths are not supported.");
#endif
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
#if UNITY
                var www = Download(path);
                return www.text;
#else
                throw new IOException("Url paths are not supported.");
#endif
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
#if UNITY
                var www = Download(path);
                return www.bytes;
#else
                throw new IOException("Url paths are not supported.");
#endif
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
            CheckLocalPath(path);
            return Directory.GetFiles(path);
        }

        public static void CreateDirectory(string path)
        {
            CheckLocalPath(path);
            Directory.CreateDirectory(path);
        }

        public static void CreateFile(string path, bool overwrite = false)
        {
            if(Exists(path) && !overwrite) 
            {
                throw new IOException ("File already exists.");
            }

            string dirPath = Path.GetDirectoryName(path);
            if(!string.IsNullOrEmpty(dirPath) && !Exists(dirPath))
            {
                CreateDirectory(dirPath);
            }
            File.Create(path).Close();
        }
        
        public static bool Delete(string path)
        {
            if(!Exists(path))
            {
                return false;
            }

            FileAttributes attributes = File.GetAttributes(path);
            if((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
                return true;
            }
            else
            {
                File.Delete(path);
                return true;
            }

        }

        private static void CheckWritablePath(string path)
        {
            if(!IsWritable(path))
            {
                throw new IOException("Path needs to be writable.");
            }
        }

        private static void CheckLocalPath(string path)
        {
            if(IsUrl(path))
            {
                throw new IOException("Path needs to be local.");
            }
        }

        public static string MakeRelativePath(string startFile, string targetFile)
        {
            StringBuilder newpath = new StringBuilder();
            
            if(startFile == null || targetFile == null)
            {
                return null;
            }
            if(startFile == targetFile)
            {
                return Path.GetFileName(targetFile);
            }
            
            var sfpath = new List<string>(startFile.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var tfpath = new List<string>(targetFile.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            
            for(int i = sfpath.Count - 1; i >= 0; i--)
            {
                if(sfpath[i] == ".")
                {
                    sfpath.RemoveAt(i);
                }
            }
            
            for(int i = tfpath.Count - 1; i >= 0; i--)
            {
                if(tfpath[i] == ".")
                {
                    tfpath.RemoveAt(i);
                }
            }
            
            int cmpdepth = Math.Min(sfpath.Count - 1, tfpath.Count - 1);
            int ixdiff = 0;
            for(; ixdiff < cmpdepth; ixdiff++)
            {
                if(false == StringComparer.OrdinalIgnoreCase.Equals(sfpath[ixdiff], tfpath[ixdiff]))
                {
                    break;
                }
            }
            
            if(ixdiff == 0 && Path.IsPathRooted(targetFile))
            {
                return targetFile;//new volumes can't be relative
            }
            
            for(int i = ixdiff; i < (sfpath.Count - 1); i++)
            {
                newpath.AppendFormat("..{0}", Path.DirectorySeparatorChar);
            }
            for(int i = ixdiff; i < tfpath.Count; i++)
            {
                newpath.Append(tfpath[i]);
                if((i + 1) < tfpath.Count)
                {
                    newpath.Append(Path.DirectorySeparatorChar);
                }
            }
            return newpath.ToString();
        }
        
        static public bool IsDirectoryEmpty(string path)
        {
            var folder = new DirectoryInfo(path);
            if(folder.Exists)
            {
                return folder.GetFileSystemInfos().Length == 0;
            }
            return false;
        }

        static public bool GlobMatch(string pattern, string value)
        {
            var deepWildcard = string.Empty+WildcardMultiChar+WildcardMultiChar;
            bool deep = pattern.Contains(deepWildcard);
            if(deep)
            {
                pattern = pattern.Replace(deepWildcard, string.Empty+WildcardMultiChar);
            }
            else if(value.Split(Path.DirectorySeparatorChar).Length != pattern.Split(Path.DirectorySeparatorChar).Length)
            {
                return false;
            }

            int pos = 0;
            while (pattern.Length != pos)
            {
                switch (pattern[pos])
                {
                case WildcardOneChar:
                    break;
                    
                case WildcardMultiChar:
                    for (int i = value.Length; i >= pos; i--)
                    {
                        if(GlobMatch(pattern.Substring(pos + 1), value.Substring(i)))
                        {
                            return true;
                        }
                    }
                    return false;
                    
                default:
                    if (value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos]))
                    {
                        return false;
                    }
                    break;
                }
                
                pos++;
            }
            return value.Length == pos;
        }
        
        static public void ReplaceFileNames(string path, string pattern, IDictionary<string,string> repls, OperationFilter dlg=null)
        {
            CheckLocalPath(path);
            var regexes = new Dictionary<Regex,string>();
            foreach(var repl in repls)
            {
                regexes.Add(new Regex(repl.Key), repl.Value);
            }
            var files = Directory.GetFiles(path);
            foreach(var src in files)
            {
                var filename = Path.GetFileName(src);
                if(!GlobMatch(pattern, filename))
                {
                    continue;
                }
                var dst = filename;
                foreach(var regex in regexes)
                {
                    dst = regex.Key.Replace(dst, regex.Value);
                }
                dst = Path.Combine(path, dst);
                if(src != dst)
                {
                    if(dlg != null && !dlg(src, dst))
                    {
                        continue;
                    }
                    File.Move(src, dst);
                }
            }
            var dirs = Directory.GetDirectories(path);
            foreach(var src in dirs)
            {
                var dir = src;
                var dirname = Path.GetFileName(src);
                if(GlobMatch(pattern, dirname))
                {
                    var dst = dirname;
                    foreach(var regex in regexes)
                    {
                        dst = regex.Key.Replace(dst, regex.Value);
                    }
                    dst = Path.Combine(path, dst);
                    if(src != dst)
                    {
                        if(dlg != null && !dlg(src, dst))
                        {
                            continue;
                        }
                        Directory.Move(src, dst);
                    }
                    dir = dst;
                }
                ReplaceFileNames(dir, pattern, repls, dlg);
            }
        }
        
        static public void ReplaceTextInFile(string path, IDictionary<string,string> repls)
        {
            string text = ReadAllText(path);
            foreach(var repl in repls)
            {
                text = new Regex(repl.Key).Replace(text, repl.Value);
            }
            WriteAllText(path, text);
        }

        static public string CleanPath(string path)
        {
            return path.TrimEnd(new char[]{ Path.DirectorySeparatorChar });
        }

        static public string[] Find(string src)
        {
            string dir;
            return Find(src, out dir);
        }

        static public string[] Find(string src, out string dirOut)
        {
            CheckLocalPath(src);
            string dir;
            string pattern;
            SearchOption search;
            if(File.Exists(src))
            {
                search = SearchOption.TopDirectoryOnly;
                dir = null;
                pattern = null;
            }
            else if(Directory.Exists(src))
            {
                search = SearchOption.AllDirectories;
                dir = src;
                pattern = string.Empty+WildcardMultiChar;
            }
            else
            {
                var deepWildcard = string.Empty+WildcardMultiChar+WildcardMultiChar;
                if(src.Contains(deepWildcard))
                {
                    src = src.Replace(deepWildcard, string.Empty+WildcardMultiChar);
                    search = SearchOption.AllDirectories;
                }
                else
                {
                    search = SearchOption.TopDirectoryOnly;
                }
                dir = GetWildcardBasePath(src);
                pattern = src;
            }

            string[] files;
            if(pattern != null && dir != null)
            {
                files = Directory.GetFiles(dir, pattern, search);
                dir = CleanPath(dir)+Path.DirectorySeparatorChar;
            }
            else
            {
                if(File.Exists(src))
                {
                    files = new string[]{ src };
                }
                else
                {
                    files = new string[0];
                }
            }

            dirOut = dir;
            return files;
        }
        
        static public void Copy(string src, string dst, OperationFilter each=null)
        {
            CheckLocalPath(dst);
            string dir;
            var files = Find(src, out dir);
            foreach(var srcPath in files)
            {
                string dstPath = dst;
                if(dir != null && srcPath.StartsWith(dir))
                {
                    var srcRelPath = srcPath.Substring(dir.Length);
                    dstPath = Path.Combine(dstPath, srcRelPath);
                }

                if(each == null || each(srcPath, dstPath))
                {
                    CopyFile(srcPath, dstPath, true);
                }
            }
        }

        static public Dictionary<string,string> Compare(string src, string dst)
        {
            return Compare(src, dst, (srcPath, dstPath) => {
                return !CompareFiles(srcPath, dstPath);
            });
        }

        static public Dictionary<string,string> Compare(string src, string dst, OperationFilter op)
        {
            var diffs = new Dictionary<string,string>();

            string srcDir;
            var srcFiles = Find(src, out srcDir);
            foreach(var srcPath in srcFiles)
            {
                string dstPath = dst;
                if(srcDir != null && srcPath.StartsWith(srcDir))
                {
                    var srcRelPath = srcPath.Substring(srcDir.Length);
                    dstPath = Path.Combine(dstPath, srcRelPath);
                }
                if(op(srcPath, dstPath))
                {
                    diffs[srcPath] = dstPath;
                }
            }

            if(srcDir != null)
            {
                string dstDir;
                var dstFiles = Find(dst, out dstDir);
                foreach(var dstPath in dstFiles)
                {
                    string srcPath = srcDir;
                    if(dstDir != null && dstPath.StartsWith(dstDir))
                    {
                        var dstRelPath = dstPath.Substring(dstDir.Length);
                        srcPath = Path.Combine(srcPath, dstRelPath);
                    }
                    if(op(srcPath, dstPath))
                    {
                        diffs[srcPath] = dstPath;
                    }
                }
            }

            return diffs;
        }

        static public bool IsWildcard(string path)
        {
            var i = path.IndexOfAny(new char[]{ WildcardOneChar, WildcardMultiChar });
            return i != -1;
        }

        static public string GetWildcardBasePath(string path)
        {
            path = CleanPath(path);
            var i = path.IndexOfAny(new char[]{ WildcardOneChar, WildcardMultiChar });
            if(i == -1)
            {
                return path;
            }
            i = path.LastIndexOf(Path.DirectorySeparatorChar, i, i+1);
            if(i == -1)
            {
                return string.Empty;
            }
            return path.Substring(0, i);
        }
        
        static public string SetDefaultFileName(string path, string filename)
        {
            if(Directory.Exists(path) || path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                return System.IO.Path.Combine(path, filename);
            }
            return path;
        }
        
        static public bool CompareFiles(string path1, string path2)
        {
            CheckLocalPath(path1);
            CheckLocalPath(path2);
            if(!File.Exists(path1))
            {
                return !File.Exists(path2);
            }
            if(!File.Exists(path2))
            {
                return !File.Exists(path1);
            }
            
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;
            
            if(path1 == path2)
            {
                return true;
            }
            
            fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read);
            
            if(fs1.Length != fs2.Length)
            {
                fs1.Close();
                fs2.Close();
                return false;
            }
            
            do
            {
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));
            
            fs1.Close();
            fs2.Close();
            
            return ((file1byte - file2byte) == 0);
        }
    }
}