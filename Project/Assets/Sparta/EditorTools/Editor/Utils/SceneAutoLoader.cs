using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SpartaTools.Editor.Utils
{
    /// <summary>
    /// Scene auto loader.
    /// </summary>
    /// <description>
    /// This class adds a Sparta > Scene Autoload menu containing options to select
    /// a "master scene" enable it to be auto-loaded when the user presses play
    /// in the editor. When enabled, the selected scene will be loaded on play,
    /// then the original scene will be reloaded on stop.
    ///
    /// Based on an idea on this thread:
    /// http://forum.unity3d.com/threads/157502-Executing-first-scene-in-build-settings-when-pressing-play-button-in-editor
    /// </description>
    [InitializeOnLoad]
    static class SceneAutoLoader
    {
        // Static constructor binds a playmode-changed callback.
        // [InitializeOnLoad] above makes sure this gets executed.
        static SceneAutoLoader()
        {
            EditorApplication.playmodeStateChanged += OnPlayModeChanged;
        }

        // Menu items to select the "master" scene and control whether or not to load it.
        [MenuItem("Sparta/Scene Autoload/Select Master Scene...", false, 1501)]
        static void SelectMasterScene()
        {
            string masterScene = EditorUtility.OpenFilePanel("Select Master Scene", Application.dataPath, "unity");
            if(!string.IsNullOrEmpty(masterScene))
            {
                MasterScene = masterScene;
                LoadMasterOnPlay = true;
            }
        }

        [MenuItem("Sparta/Scene Autoload/Load Master On Play", true)]
        static bool ShowLoadMasterOnPlay()
        {
            return !LoadMasterOnPlay;
        }

        [MenuItem("Sparta/Scene Autoload/Load Master On Play", false, 1502)]
        static void EnableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = true;
        }

        [MenuItem("Sparta/Scene Autoload/Don't Load Master On Play", true)]
        static bool ShowDontLoadMasterOnPlay()
        {
            return LoadMasterOnPlay;
        }

        [MenuItem("Sparta/Scene Autoload/Don't Load Master On Play", false, 1503)]
        static void DisableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = false;
        }

        // Play mode change callback handles the scene load/reload.
        static void OnPlayModeChanged()
        {
            if(!LoadMasterOnPlay)
            {
                return;
            }

            if(!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed play -- autoload master scene.
                PreviousScene = SceneManager.GetActiveScene().path;
                if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    var scene = EditorSceneManager.OpenScene(MasterScene);
                    if(!scene.IsValid())
                    {
                        Debug.LogError(string.Format("error: scene not found: {0}", MasterScene));
                        EditorApplication.isPlaying = false;
                    }
                }
                else
                {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }

            // isPlaying check required because cannot OpenScene while playing
            if(!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed stop -- reload previous scene.
                var scene = EditorSceneManager.OpenScene(PreviousScene);
                if(!scene.IsValid())
                {
                    Debug.LogError(string.Format("error: scene not found: {0}", PreviousScene));
                }
            }
        }

        // Properties are remembered as editor preferences.
        const string EditorPrefLoadMasterOnPlay = "SceneAutoLoader.LoadMasterOnPlay";
        const string EditorPrefMasterScene = "SceneAutoLoader.MasterScene";
        const string EditorPrefPreviousScene = "SceneAutoLoader.PreviousScene";

        static bool LoadMasterOnPlay
        {
            get 
            {
                
                return EditorPrefs.GetBool(PlayerSettings.productName + EditorPrefLoadMasterOnPlay, false); 
            }
            set
            {
                EditorPrefs.SetBool(PlayerSettings.productName + EditorPrefLoadMasterOnPlay, value); 
            }
        }

        static string MasterScene
        {
            get
            {
                return EditorPrefs.GetString(PlayerSettings.productName + EditorPrefMasterScene, "Sparta/SocialPoint/IntroAnimation/IntroAnimation.unity"); 
            }
            set
            {
                EditorPrefs.SetString(PlayerSettings.productName + EditorPrefMasterScene, value); 
            }
        }

        static string PreviousScene
        {
            get 
            {
                return EditorPrefs.GetString(PlayerSettings.productName + EditorPrefPreviousScene, SceneManager.GetActiveScene().path); 
            }
            set
            {
                EditorPrefs.SetString(PlayerSettings.productName + EditorPrefPreviousScene, value); 
            }
        }
    }
}