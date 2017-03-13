using System.IO;
using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.View
{
    public static class EditorToolsWindow
    {
        static readonly string[] TempFolders = new string[] { "Temp", "obj", "Library" };

        [MenuItem("Sparta/Editor/Restart &#r", false, 1000)]
        public static void RestartEditor()
        {
            Restart();   
        }

        [MenuItem("Sparta/Editor/Clean", false, 1000)]
        public static void CleanEditorLibrary()
        {
            if(EditorUtility.DisplayDialog("Clean Library Folder", "Editor must be restarted to apply changes in native plugins", "Clean and restart", "Cancel"))
            {
                Clean();
            }
        }

        static void Clean()
        {
            for(var i = 0; i < TempFolders.Length; ++i)
            {
                var path = Path.Combine(Application.dataPath, "../" + TempFolders[i]);
                Directory.Delete(path, true);
            }
            
            Restart();
        }

        static void Restart()
        {
            EditorApplication.OpenProject(Path.Combine(Application.dataPath, ".."));
        }
	}
}
