using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
	public static class SPAMResources
	{
        private static string resourcesAssetsPath = SPAMResources.FindResourcesFolder();

        private static TLAnimatedImage _loadingAtlas;
        private static TLAnimatedImage _loadingAtlasSml;

		public static TLAnimatedImage loadingAtlas { get { return new TLAnimatedImage(_loadingAtlas); } }
        public static TLAnimatedImage loadingAtlasSml { get { return new TLAnimatedImage(_loadingAtlasSml); } }

		public static TLImage progressBarFrame;
		public static TLImage progressBarFill;
		
		static SPAMResources ()
		{
            if (!Directory.Exists (resourcesAssetsPath))
			{
				Debug.LogWarning ("The sp-tool-lib-unityclient resources folder path is incorrect. If you relocated the tools please change the path accordingly.");
			}

			Texture2D[] _loadingAtlasTex;
            Texture2D[] _loadingAtlasSmlTex;

			if (EditorGUIUtility.isProSkin) {
                _loadingAtlasTex = TLTexturePool.LoadImage ("loadingAtlas", Path.Combine (resourcesAssetsPath, "loading_atlas-darktheme.png"), TLTextureType.SquareHorizontalAtlas, new int[] {});
                _loadingAtlasSmlTex = TLTexturePool.LoadImage ("loadingAtlasSml", Path.Combine (resourcesAssetsPath, "loading_atlas-darktheme.png"), TLTextureType.SquareHorizontalAtlas, new int[] {16, 16});
			} else {
                _loadingAtlasTex = TLTexturePool.LoadImage ("loadingAtlas", Path.Combine (resourcesAssetsPath, "loading_atlas-lighttheme.png"), TLTextureType.SquareHorizontalAtlas, new int[] {});
                _loadingAtlasSmlTex = TLTexturePool.LoadImage ("loadingAtlasSml", Path.Combine (resourcesAssetsPath, "loading_atlas-lighttheme.png"), TLTextureType.SquareHorizontalAtlas, new int[] {16, 16});
			}

			_loadingAtlas = new TLAnimatedImage (_loadingAtlasTex);
			_loadingAtlas.IsLoopable = true;
			_loadingAtlas.SetFrameRate (10/*fps*/);

            _loadingAtlasSml = new TLAnimatedImage (_loadingAtlasSmlTex);
            _loadingAtlasSml.IsLoopable = true;
            _loadingAtlasSml.SetFrameRate (10);

            progressBarFrame = new TLImage (TLTexturePool.LoadImage ("progressBarFrame", Path.Combine (resourcesAssetsPath, "progress_bar_frame.png"), TLTextureType.Single, new int[] {})[0]);
            progressBarFill = new TLImage (TLTexturePool.LoadImage ("progressBarFill", Path.Combine (resourcesAssetsPath, "progress_bar_fill.png"), TLTextureType.Single, new int[] {})[0]);
		}

        private static string FindResourcesFolder()
        {
            // search for the path of a given icon
            string sampleIcon = "loading_atlas-lighttheme";
            
            string[] guids = AssetDatabase.FindAssets("" + sampleIcon + " t:texture2D");
            if (guids.Length > 1) {
                Debug.LogWarning (String.Format("Could not find the Resources folder from '{0}' icon file. There are more than one file.", sampleIcon));
                return "";
            } else if (guids.Length < 1) {
                Debug.LogWarning (String.Format("Could not find the Resources folder from '{0}' icon file. No file could be found.", sampleIcon));
                return "";
            }
            
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            
            return Path.GetDirectoryName(assetPath) + "/";
        }

        public static void Reimport()
        {
            Texture2D[] _loadingAtlasTex;
            Texture2D[] _loadingAtlasSmlTex;
            
            if (EditorGUIUtility.isProSkin) {
                _loadingAtlasTex = TLTexturePool.GetImages ("loadingAtlas");
                _loadingAtlasSmlTex = TLTexturePool.GetImages ("loadingAtlasSml");
            } else {
                _loadingAtlasTex = TLTexturePool.GetImages ("loadingAtlas");
                _loadingAtlasSmlTex = TLTexturePool.GetImages ("loadingAtlasSml");
            }
            
            _loadingAtlas.SetFrames(_loadingAtlasTex, _loadingAtlas.fps);
            _loadingAtlasSml.SetFrames(_loadingAtlasSmlTex, _loadingAtlasSml.fps);
            
            progressBarFrame = new TLImage (TLTexturePool.GetImages ("progressBarFrame")[0]);
            progressBarFill = new TLImage (TLTexturePool.GetImages ("progressBarFill")[0]);
        }
	}
}

