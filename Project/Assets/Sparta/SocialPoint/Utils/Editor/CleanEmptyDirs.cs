using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.Utils
{
    [InitializeOnLoad]
    public class CleanEmptyDirs : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            List<DirectoryInfo> emptyDirs;
            FillEmptyDirList(out emptyDirs);
            if(emptyDirs != null && emptyDirs.Count > 0)
            {
                DeleteAllEmptyDirAndMeta(ref emptyDirs);

                Debug.Log("[Clean] Cleaned Empty Directories on Save");
            }

            return paths;
        }

        public static void DeleteAllEmptyDirAndMeta(ref List<DirectoryInfo> emptyDirs)
        {
            for(int i = 0, emptyDirsCount = emptyDirs.Count; i < emptyDirsCount; i++)
            {
                var dirInfo = emptyDirs[i];
                AssetDatabase.MoveAssetToTrash(GetRelativePathFromCd(dirInfo.FullName));
            }
            emptyDirs = null;
        }

        public static void FillEmptyDirList(out List<DirectoryInfo> emptyDirs)
        {
            var newEmptyDirs = new List<DirectoryInfo>();
            emptyDirs = newEmptyDirs;

            var assetDir = new DirectoryInfo(Application.dataPath);

            WalkDirectoryTree(assetDir, (dirInfo, areSubDirsEmpty) => {
                bool isDirEmpty = areSubDirsEmpty && DirHasNoFile(dirInfo);
                if(isDirEmpty)
                {
                    newEmptyDirs.Add(dirInfo);
                } 
                return isDirEmpty;
            });
        }

        // return: Is this directory empty?
        delegate bool IsEmptyDirectory(DirectoryInfo dirInfo, bool areSubDirsEmpty);

        // return: Is this directory empty?
        static bool WalkDirectoryTree(DirectoryInfo root, IsEmptyDirectory pred)
        {
            DirectoryInfo[] subDirs = root.GetDirectories();

            bool areSubDirsEmpty = true;
            for(int i = 0, subDirsLength = subDirs.Length; i < subDirsLength; i++)
            {
                DirectoryInfo dirInfo = subDirs[i];
                areSubDirsEmpty &= WalkDirectoryTree(dirInfo, pred);
            }

            bool isRootEmpty = pred(root, areSubDirsEmpty);
            return isRootEmpty;
        }

        static bool DirHasNoFile(DirectoryInfo dirInfo)
        {
            FileInfo[] files = null;
            var filesList = new List<FileInfo>();

            try
            {
                files = dirInfo.GetFiles("*.*");
                for(int i = 0, filesLength = files.Length; i < filesLength; i++)
                {
                    var f = files[i];
                    if(!IsMetaFile(f.Name))
                    {
                        filesList.Add(f);
                    }
                }
                files = filesList.ToArray();
            }
            catch(Exception)
            {
            } 

            return files == null || files.Length == 0;
        }

        static string GetRelativePathFromCd(string filespec)
        {
            return GetRelativePath(filespec, Directory.GetCurrentDirectory());
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            var pathUri = new Uri(filespec);
            // Folders must end in a slash
            if(!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        static bool IsMetaFile(string path)
        {
            return path.EndsWith(".meta");
        }
    }
}
    
    