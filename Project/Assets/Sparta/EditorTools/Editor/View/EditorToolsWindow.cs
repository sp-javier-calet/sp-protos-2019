using System.IO;
using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.View
{
    public static class EditorToolsWindow
    {
        static readonly string[] TempFolders = new string[] { "obj", "Library" };

        [MenuItem("Sparta/Editor/Restart &#r", false, 1000)]
        public static void RestartEditor()
        {
            Restart();   
        }

        [MenuItem("Sparta/Editor/Clean", false, 1000)]
        public static void CleanEditorLibrary()
        {
            if(EditorUtility.DisplayDialog("Clean Library Folder", "A full reimport will be required after restart. Do you want to continue?", "Clean and restart", "Cancel"))
            {
                Clean();
            }
        }

        static void Clean()
        {
            for(var i = 0; i < TempFolders.Length; ++i)
            {
                var path = Path.Combine(ProjectRoot, TempFolders[i]);
                Directory.Delete(path, true);
            }
            
            Restart();
        }

        static void Restart()
        {
            EditorApplication.OpenProject(ProjectRoot);
        }

        static string ProjectRoot
        {
            get
            {
                return Path.Combine(Application.dataPath, "..");
            }
        }
    }
}
