using UnityEngine;
using UnityEditor;
using System.Collections;
using SocialPoint.Tool.Shared.TLGUI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SocialPoint.Editor.LevelCaptureTool
{
    public class GUILevelCaptureToolController : TLController
    {
        GuiLevelCaptureToolView View { get { return (GuiLevelCaptureToolView)_view; } }
        GuiLevelCaptureToolModel Model { get { return (GuiLevelCaptureToolModel)_model; } }

        public GUILevelCaptureToolController(TLView view, TLModel model): base ( view, model )
        {
            Init();
            TryToInitBundleManager();
        }

        void Init()
        {
            View.btSelectFolder.onPathSelectedEvent.Connect(OnFolderSelected);
            View.btCapture.onClickEvent.Connect(OnCapture);

            //Set up on load
            OnLoadActions += SetUpView;
        }

        void OnFolderSelected()
        {
            View.tfOutputPath.text = View.btSelectFolder.selectedPath;
        }

        string[] GetAllScenes()
        {
            return AssetDatabase.FindAssets("t:Scene").Select(a => AssetDatabase.GUIDToAssetPath(a)).ToArray();
        }

        void SetUpView()
        {
            var scenes = GetAllScenes();
            View.twLevelSelector.SetListItems(scenes, sorted: true);
        }

        /// <summary>
        /// If BundleManager is defined, add functionality to select scenes from loaded bundles
        /// </summary>
        void TryToInitBundleManager()
        {
            if(Utils.IsBundleManagerLoaded())
            {
                View.btSelectFromBundles.onClickEvent.Connect(OnSelectScenesFromBundleManager);
            }
        }

        void OnSelectScenesFromBundleManager()
        {
            if(Utils.IsBundleManagerLoaded())
            {
                var bundleScenePaths = Utils.GetSceneAssetPathsFomBundles();
                View.twLevelSelector.SelectLevels(bundleScenePaths);
            }
        }
        //

        void OnCapture()
        {
			if(View.tfOutputPath.text.Equals(string.Empty))
            {
                EditorUtility.DisplayDialog("Cannot proceed", "Please, specify a folder to save the captures first.", "Ok");
                return;
            }

			if(int.Parse(View.nfWidth.number) <= 0 || int.Parse(View.nfHeight.number) <= 0)
			{
				EditorUtility.DisplayDialog("Cannot proceed", "Resolution with/height have zero or negative values, please change them to positive.", "Ok");
				return;
			}
            
            if(UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
				string[] sceneAssetPaths = View.twLevelSelector.GetSelectedLevels();
				RenderTexture rt = null;
				Texture2D screenShot = null;

				for(int i = 0; i < sceneAssetPaths.Length; ++i)
				{
					var progress = (float)i / (float)sceneAssetPaths.Length;
					if(!EditorUtility.DisplayCancelableProgressBar("Capture Levels", String.Format("Taking snapshot for {0} ...", Path.GetFileNameWithoutExtension(sceneAssetPaths[i])), progress))
					{
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneAssetPaths[i]);
						Utils.CaptureCurrenScene(Camera.main, View.tfOutputPath.text, ref rt, ref screenShot, int.Parse(View.nfWidth.number), int.Parse(View.nfHeight.number));
					}
					else
					{
						Debug.Log ("Progress cancelled by the user.");
						break;
					}
				}

				EditorUtility.ClearProgressBar();

				if(rt)
				{
					UnityEngine.Object.DestroyImmediate(rt);
				}
				if(screenShot)
				{
					UnityEngine.Object.DestroyImmediate(screenShot);
				}
            }
        }
    }
}