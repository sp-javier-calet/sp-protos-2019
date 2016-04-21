using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Commonly used Texture2D for icons.
    /// </summary>
    /// This class should be reimplemented using TLImage instead of Texture2D but can be used as well.
	public static class TLIcons
	{
		private static string iconsAssetsPath = TLIcons.FindIconsFolder();

		public static readonly Texture2D failImg;
		public static readonly Texture2D ignoreImg;
		public static readonly Texture2D playSelectedImg;
		public static readonly Texture2D rerunImg;
		public static readonly Texture2D playImg;
		public static readonly Texture2D successImg;
		public static readonly Texture2D unknownImg;
		public static readonly Texture2D inconclusiveImg;
		public static readonly Texture2D stopwatchImg;
		public static readonly Texture2D plusImg;
		public static readonly Texture2D gearImg;
		public static readonly Texture2D plusWhiteImg;
		public static readonly Texture2D plusGreenImg;
		public static readonly Texture2D plusRedImg;
		public static readonly Texture2D minusWhiteImg;
		public static readonly Texture2D minusGreenImg;
		public static readonly Texture2D minusRedImg;
		public static readonly Texture2D tickImg;
		public static readonly Texture2D crossImg;
		public static readonly Texture2D warningImg;
		public static readonly Texture2D shareImg;
		public static readonly Texture2D insetFrameImg;
		public static readonly Texture2D contractImg;
		public static readonly Texture2D lockImg;
		public static readonly Texture2D unlockImg;
		public static readonly Texture2D lockForbImg;
		public static readonly Texture2D downloadImg;
		public static readonly Texture2D uploadImg;
		public static readonly Texture2D compareImg;
		public static readonly Texture2D undoImg;
		public static readonly Texture2D reloadImg;
		public static readonly Texture2D shareWarnImg;
        public static readonly Texture2D frameHeaderImg;
        public static readonly Texture2D powerImg;

		static TLIcons ()
		{
			if (!Directory.Exists (iconsAssetsPath))
			{
                Debug.LogWarning ("The SocialPoint.Tool.Shared.TLGUI.TLIcons asset folder path is incorrect.");
			}

			failImg = LoadTexture ("failed.png");
			ignoreImg = LoadTexture("ignored.png");
			successImg = LoadTexture("passed.png");
			unknownImg = LoadTexture("normal.png");
			inconclusiveImg = LoadTexture("inconclusive.png");
			stopwatchImg = LoadTexture("stopwatch.png");
			plusWhiteImg  = LoadTexture("plus_white.png");
			plusGreenImg  = LoadTexture("plus_green.png");
			plusRedImg    = LoadTexture("plus_red.png");
			minusWhiteImg = LoadTexture("minus_white.png");
			minusGreenImg = LoadTexture("minus_green.png");
			minusRedImg   = LoadTexture("minus_red.png");
			tickImg = LoadTexture("tick.png");
			crossImg = LoadTexture("cross.png");
			warningImg = LoadTexture("warning.png");
			shareImg = LoadTexture("share.png");
			lockImg = LoadTexture ("lock.png");
			unlockImg = LoadTexture ("unlock.png");
			lockForbImg = LoadTexture ("lock_forbidden.png");
			downloadImg = LoadTexture ("download_arrow.png");
			uploadImg = LoadTexture ("upload_arrow.png");
			compareImg = LoadTexture ("compare_arrows.png");
			undoImg = LoadTexture ("undo_arrow.png");
			reloadImg = LoadTexture ("reload_arrow.png");
			shareWarnImg = LoadTexture ("share_warn.png");
            powerImg = LoadTexture ("power.png");

			insetFrameImg = LoadUncompressedTexture("inset_frame_border.png");
			insetFrameImg.filterMode = FilterMode.Point;
            frameHeaderImg = LoadUncompressedTexture("frame_header.png");
            frameHeaderImg.filterMode = FilterMode.Point;

			if (EditorGUIUtility.isProSkin)
			{
				playImg = LoadTexture ("play-darktheme.png");
				playSelectedImg = LoadTexture ("play_selected-darktheme.png");
				rerunImg = LoadTexture ("rerun-darktheme.png");
				plusImg = LoadTexture ("create-darktheme.png");
				gearImg = LoadTexture ("options-darktheme.png");
				contractImg = LoadTexture("contract-darktheme.png");
			}
			else
			{
				playImg = LoadTexture ("play-lighttheme.png");
				playSelectedImg = LoadTexture ("play_selected-lighttheme.png");
				rerunImg = LoadTexture ("rerun-lighttheme.png");
				plusImg = LoadTexture ("create-lighttheme.png");
				gearImg = LoadTexture ("options-lighttheme.png");
				contractImg = LoadTexture("contract-lighttheme.png");
			}
		}

		private static Texture2D LoadTexture (string fileName)
		{
			return (Texture2D)AssetDatabase.LoadAssetAtPath (iconsAssetsPath + fileName, typeof (Texture2D));
		}

		private static Texture2D LoadUncompressedTexture (string fileName)
		{
			string imagePath = iconsAssetsPath + fileName;
			TextureImporter ti = TextureImporter.GetAtPath (imagePath) as TextureImporter;
			ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			AssetDatabase.ImportAsset (imagePath);

			return (Texture2D)AssetDatabase.LoadAssetAtPath (imagePath, typeof (Texture2D));
		}

		private static string FindIconsFolder()
		{
			// search for the path of a given icon
			string sampleIcon = "inset_frame_border";

			string[] guids = AssetDatabase.FindAssets("" + sampleIcon + " t:texture2D");
			if (guids.Length > 1) {
				Debug.LogWarning (String.Format("Could not find the Icons folder from '{0}' icon file. There are more than one file.", sampleIcon));
				return "";
			} else if (guids.Length < 1) {
				Debug.LogWarning (String.Format("Could not find the Icons folder from '{0}' icon file. No file could be found.", sampleIcon));
				return "";
			}

			string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);

			return Path.GetDirectoryName(assetPath) + "/";
		}
	}
}
