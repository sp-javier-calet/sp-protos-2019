using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SocialPoint.IO;
using SocialPoint.Base;
using System;

namespace SocialPoint.Exporter
{
    public abstract class SceneExporter : BaseExporter
    {
        public UnityEngine.Object Scene;

        public override sealed void Export(IFileManager files, Log.ILogger log)
        {
            if(Scene == null)
            {
                throw new InvalidOperationException("Scene is not configured");
            }

            string previousScene = EditorSceneManager.GetActiveScene().path;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                throw new InvalidOperationException("Cannot switch scene");
            }
            try
            {
                string pathToScene = AssetDatabase.GetAssetPath(Scene);
                EditorSceneManager.OpenScene(pathToScene);
                ExportScene(files, log);
            }
            finally
            {
                EditorSceneManager.OpenScene(previousScene);
            }
        }

        protected abstract void ExportScene(IFileManager files, Log.ILogger log);
    }
}