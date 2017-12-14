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
        static DirectoryInfo _assetDir = new DirectoryInfo(Application.dataPath);
        static List<DirectoryInfo> _emptyDirs = new List<DirectoryInfo>();
        static List<FileInfo> _filesList = new List<FileInfo>();

        public static string[] OnWillSaveAssets(string[] paths)
        {
            FillEmptyDirList();
            if(_emptyDirs != null && _emptyDirs.Count > 0)
            {
                DeleteAllEmptyDirAndMeta();

                Debug.Log("[Clean] Cleaned Empty Directories on Save");
            }

            return paths;
        }

        public static void DeleteAllEmptyDirAndMeta()
        {
            for(int i = 0, emptyDirsCount = _emptyDirs.Count; i < emptyDirsCount; i++)
            {
                var dirInfo = _emptyDirs[i];
                AssetDatabase.MoveAssetToTrash(FileUtil.GetProjectRelativePath(dirInfo.FullName));
            }
        }

        public static void FillEmptyDirList()
        {
            _emptyDirs.Clear();

            WalkDirectoryTree(_assetDir, (dirInfo, areSubDirsEmpty) => {
                bool isDirEmpty = areSubDirsEmpty && DirHasNoFile(dirInfo);
                if(isDirEmpty)
                {
                    _emptyDirs.Add(dirInfo);
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
            _filesList.Clear();

            try
            {
                files = dirInfo.GetFiles("*.*");
                for(int i = 0, filesLength = files.Length; i < filesLength; i++)
                {
                    var f = files[i];
                    if(!IsMetaFile(f.Name) && !IsDSStoreFile(f.Name))
                    {
                        _filesList.Add(f);
                    }
                }
                files = _filesList.ToArray();
            }
            catch(Exception)
            {
            }

            return files == null || files.Length == 0;
        }

        static bool IsMetaFile(string path)
        {
            return path.EndsWith(".meta");
        }

        static bool IsDSStoreFile(string path)
        {
            return path.EndsWith(".DS_Store");
        }
    }
}
    
    