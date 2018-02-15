using UnityEditor;
using UnityEditor.SceneManagement;
using SocialPoint.IO;
using SocialPoint.Base;
using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace SocialPoint.Exporter
{
    public abstract class SceneExporter : BaseExporter
    {
        public const string BasePath = "Server";
        public bool AppendSceneNameToBasePath = true;
        public UnityEngine.Object Scene;

        public Scene ExportingScene
        {
            get
            {
                if(Scene == null)
                {
                    return SceneManager.GetActiveScene();
                }
                else
                {
                    var sceneAsset = Scene as SceneAsset;
                    return SceneManager.GetSceneByName(sceneAsset.name);
                }
            }
        }

        public string ExportingScenePath
        {
            get
            {
                return ExportingScene.path;
            }
        }

        protected string SceneName
        {
            get
            {
                return ExportingScene.name;
            }
        }

        public string GetPathExportName(string exportName)
        {
            var finalName = AppendSceneNameToBasePath && !string.IsNullOrEmpty(SceneName) ? Path.Combine(SceneName, exportName) : exportName;
            finalName = !string.IsNullOrEmpty(BasePath) ? Path.Combine(BasePath, finalName) : finalName;
            return finalName;
        }

        public override sealed void Export(IFileManager files, Log.ILogger log)
        {
            string previousScene = EditorSceneManager.GetActiveScene().path;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                throw new InvalidOperationException("Cannot switch scene");
            }
            try
            {
                string pathToScene = ExportingScenePath;
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