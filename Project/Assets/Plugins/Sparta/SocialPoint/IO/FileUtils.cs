#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE
#define UNITY
#endif

#if UNITY
using UnityEngine;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.IO
{
    public sealed class FileUtils
    {
        #if UNITY

        static WWW Download(string path)
        {
            var www = new WWW(path);
            while(!www.isDone)
            {
            }
            return www;
        }

        static bool OpenPath(string path)
        {
            var www = Download(path);
            bool exists = string.IsNullOrEmpty(www.error);
            www.Dispose();
            return exists;
        }

        static byte[] ReadBytes(string path)
        {
            var www = Download(path);
            var bytes = www.bytes;
            www.Dispose();
            return bytes;
        }

        static string ReadText(string path)
        {
            var www = Download(path);
            var text = www.text;
            www.Dispose();
            return text;
        }

        #else
        
        static bool OpenPath(string path)
        {
            throw new IOException("Url paths are not supported.");
        }

        static byte[] ReadBytes(string path)
        {
            throw new IOException("Url paths are not supported.");
        }

        static string ReadText(string path)
        {
            throw new IOException("Url paths are not supported.");
        }

        #endif


        public delegate bool OperationFilter(string src, string dst);

        public static bool IsBinary(string path)
        {
            var content = ReadAllBytes(path);
            for(int i = 1; i < 512 && i < content.Length; i++)
            {
                if(content[i] == 0x00 && content[i - 1] == 0x00)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsWritable(string path)
        {
            if(IsUrl(path))
            {
                return false;
            }
            else
            {
                var info = new FileInfo(path);
                return !info.Exists || !info.IsReadOnly; // FIXME IF file doesnt exist, check folder permissions
            }
        }

        public static string Combine(string basePath, string relPath)
        {
            return IsUrl(basePath) ? new Uri(new Uri(basePath), relPath).AbsoluteUri : Path.Combine(basePath, relPath);
        }

        public static bool CopyFile(string from, string to, bool overwrite = false)
        {
            if(ExistsFile(to))
            {
                if(!overwrite)
                {
                    return false;
                }
            }
            
            CheckWritablePath(to);
            
            var bytes = ReadAllBytes(from);
            WriteAllBytes(to, bytes);
            return true;
        }

        public static bool ExistsFile(string path)
        {
            if(IsUrl(path))
            {
                return OpenPath(path);
            }
            return File.Exists(path);
        }

        public static bool ExistsDirectory(string path)
        {
            if(IsUrl(path))
            {
                return OpenPath(path);
            }
            return Directory.Exists(path);
        }

        public static long FileSize(string path)
        {
            if(IsUrl(path))
            {
                return (new FileInfo(path)).Length;
            }
            return 0L;
        }

        public static bool IsUrl(string path)
        {
            return path.Contains("://");
        }

        public static string ReadAllText(string path)
        {
            if(IsUrl(path))
            {
                return ReadText(path);
            }
            return File.ReadAllText(path);
        }

        public static byte[] ReadAllBytes(string path)
        {
            if(IsUrl(path))
            {
                return ReadBytes(path);
            }
            return File.ReadAllBytes(path);
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
            path = Path.GetFullPath(path);

            string[] files = null;

            try
            {
                files = Directory.GetFiles(path);
            }
            catch(Exception e)
            {
                CatchException(e);
            }

            return files;
        }

        public static void CreateDirectory(string path)
        {
            if(!ExistsDirectory(path))
            {
                CheckLocalPath(path);
                path = Path.GetFullPath(path);
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateFile(string path, bool overwrite = false)
        {
            if(ExistsFile(path) && !overwrite)
            {
                throw new IOException("File already exists.");
            }

            string dirPath = Path.GetDirectoryName(path);
            if(!string.IsNullOrEmpty(dirPath) && !ExistsDirectory(dirPath))
            {
                CreateDirectory(dirPath);
            }
            File.Create(path).Close();
        }

        public static bool DeleteFile(string path)
        {
            if(IsUrl(path))
            {
                return false;
            }
            if(!ExistsFile(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }

        public static bool DeleteDirectory(string path)
        {
            if(IsUrl(path))
            {
                return false;
            }
            if(!ExistsDirectory(path))
            {
                return false;
            }

            Directory.Delete(path, true);
            return true;
        }

        static void CheckWritablePath(string path)
        {
            if(!IsWritable(path))
            {
                throw new IOException("Path needs to be writable.");
            }
        }

        static void CheckLocalPath(string path)
        {
            if(IsUrl(path))
            {
                throw new IOException("Path needs to be local.");
            }
        }

        public static string MakeRelativePath(string startFile, string targetFile)
        {
            var newpath = new StringBuilder();
            
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
                if(!StringComparer.OrdinalIgnoreCase.Equals(sfpath[ixdiff], tfpath[ixdiff]))
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
            return StringUtils.GlobMatch(pattern, value);
        }

        static public void ReplaceFileNames(string path, string pattern, IDictionary<string,string> repls, OperationFilter dlg = null)
        {
            CheckLocalPath(path);
            var regexes = new Dictionary<Regex,string>();
            var itr = repls.GetEnumerator();
            while(itr.MoveNext())
            {
                var repl = itr.Current;
                regexes.Add(new Regex(repl.Key), repl.Value);
            }
            itr.Dispose();

            var files = Directory.GetFiles(path);
            for(int i = 0, filesLength = files.Length; i < filesLength; i++)
            {
                var src = files[i];
                var filename = Path.GetFileName(src);
                if(!GlobMatch(pattern, filename))
                {
                    continue;
                }
                var dst = filename;
                var itr2 = regexes.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var regex = itr2.Current;
                    dst = regex.Key.Replace(dst, regex.Value);
                }
                itr2.Dispose();

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
            for(int i = 0, dirsLength = dirs.Length; i < dirsLength; i++)
            {
                var src = dirs[i];
                var dir = src;
                var dirname = Path.GetFileName(src);
                if(GlobMatch(pattern, dirname))
                {
                    var dst = dirname;
                    var itr2 = regexes.GetEnumerator();
                    while(itr2.MoveNext())
                    {
                        var regex = itr2.Current;
                        dst = regex.Key.Replace(dst, regex.Value);
                    }
                    itr2.Dispose();

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

            var itr = repls.GetEnumerator();
            while(itr.MoveNext())
            {
                var repl = itr.Current;
                text = new Regex(repl.Key).Replace(text, repl.Value);
            }
            itr.Dispose();

            WriteAllText(path, text);
        }

        static public string CleanPath(string path)
        {
            return path.TrimEnd(new []{ Path.DirectorySeparatorChar });
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
            if(ExistsFile(src))
            {
                search = SearchOption.TopDirectoryOnly;
                dir = null;
                pattern = null;
            }
            else if(ExistsDirectory(src))
            {
                search = SearchOption.AllDirectories;
                dir = src;
                pattern = StringUtils.WildcardMultiChar.ToString();
            }
            else
            {
                if(src.Contains(StringUtils.WildcardDeep))
                {
                    src = src.Replace(StringUtils.WildcardDeep, StringUtils.WildcardMultiChar.ToString());
                    search = SearchOption.AllDirectories;
                }
                else
                {
                    search = SearchOption.TopDirectoryOnly;
                }
                dir = GetWildcardBasePath(src);
                pattern = src.Length > dir.Length ? src.Substring(dir.Length + 1) : StringUtils.WildcardMultiChar.ToString();
            }

            string[] files;
            if(!string.IsNullOrEmpty(pattern) && !string.IsNullOrEmpty(dir) && ExistsDirectory(dir))
            {
                files = Directory.GetFiles(dir, pattern, search);
                dir = CleanPath(dir) + Path.DirectorySeparatorChar;
            }
            else
            {
                files = ExistsFile(src) ? new[] {
                    src
                } : new string[0];
            }

            dirOut = dir;
            return files;
        }

        static public void Copy(string src, string dst, OperationFilter each = null)
        {
            CheckLocalPath(dst);
            string dir;
            var files = Find(src, out dir);
            for(int i = 0, filesLength = files.Length; i < filesLength; i++)
            {
                var srcPath = files[i];
                string dstPath = dst;
                if(dir != null && StringUtils.StartsWith(srcPath, dir))
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
            return Compare(src, dst, (srcPath, dstPath) => !CompareFiles(srcPath, dstPath));
        }

        static public Dictionary<string,string> Compare(string src, string dst, OperationFilter op)
        {
            return Compare(src, dst, true, op);
        }

        static public Dictionary<string,string> CompareSource(string src, string dst, OperationFilter op)
        {            
            return Compare(src, dst, false, op);
        }

        static public Dictionary<string,string> CompareSource(string src, string dst)
        {            
            return Compare(src, dst, false, (srcPath, dstPath) => !CompareFiles(srcPath, dstPath));
        }

        static public Dictionary<string,string> Compare(string src, string dst, bool checkDst, OperationFilter op)
        {
            var diffs = new Dictionary<string,string>();

            string srcDir;
            var srcFiles = Find(src, out srcDir);
            for(int i = 0, srcFilesLength = srcFiles.Length; i < srcFilesLength; i++)
            {
                var srcPath = srcFiles[i];
                string dstPath = dst;
                if(srcDir != null && StringUtils.StartsWith(srcPath, srcDir))
                {
                    var srcRelPath = srcPath.Substring(srcDir.Length);
                    dstPath = Path.Combine(dstPath, srcRelPath);
                }
                if(op(srcPath, dstPath))
                {
                    diffs[srcPath] = dstPath;
                }
            }

            if(checkDst && srcDir != null)
            {
                if(ExistsDirectory(dst))
                {
                    if(IsWildcard(src))
                    {
                        var srcBase = GetWildcardBasePath(src);
                        var srcWild = StringUtils.WildcardOneChar.ToString();
                        if(src.Length > srcBase.Length)
                        {
                            srcWild = src.Substring(srcBase.Length + 1);
                        }
                        dst = Path.Combine(dst, srcWild);
                    }
                }
                string dstDir;
                var dstFiles = Find(dst, out dstDir); 
                for(int i = 0, dstFilesLength = dstFiles.Length; i < dstFilesLength; i++)
                {
                    var dstPath = dstFiles[i];
                    string srcPath = srcDir;
                    if(dstDir != null && StringUtils.StartsWith(dstPath, dstDir))
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
            return StringUtils.IsWildcard(path);
        }

        static public string GetWildcardBasePath(string path)
        {
            path = CleanPath(path);
            var i = path.IndexOfAny(new []{ StringUtils.WildcardOneChar, StringUtils.WildcardMultiChar });
            if(i == -1)
            {
                return path;
            }
            i = path.LastIndexOf(Path.DirectorySeparatorChar, i, i + 1);
            return i == -1 ? string.Empty : path.Substring(0, i);
        }

        static public string SetDefaultFileName(string path, string filename)
        {
            if(ExistsDirectory(path) || StringUtils.EndsWith(path, Path.DirectorySeparatorChar.ToString()))
            {
                return Path.Combine(path, filename);
            }
            return path;
        }

        static public bool CompareFiles(string path1, string path2)
        {
            CheckLocalPath(path1);
            CheckLocalPath(path2);
            if(!ExistsFile(path1))
            {
                return !ExistsFile(path2);
            }
            if(!ExistsFile(path2))
            {
                return !ExistsFile(path1);
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

                fs1.Dispose();
                fs2.Dispose();

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

            fs1.Dispose();
            fs2.Dispose();
            
            return ((file1byte - file2byte) == 0);
        }

        static void CatchException(Exception e)
        {
            Log.x(e);
            DebugUtils.Stop();
        }
    }
}
