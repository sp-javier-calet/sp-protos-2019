using System;
using UnityEditor;
using UnityEngine;

namespace FyberEditor
{
	public class UnityAdsAssetMenu
	{

#if !UNITY_5
		[MenuItem("Fyber/Mediation Assets Generation/UnityAds")]
		public static void GenerateUnityAdsAsset()
		{
			var asset = ScriptableObject.CreateInstance<UnityAdsBundleDefinition>();
			AssetDatabase.CreateAsset(asset, "Assets/Fyber/Editor/Resources/Mediation/UnityAds/UnityAdsFyberBundle.asset");
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Fyber/Mediation Assets Generation/UnityAds", true)]
		public static bool GenerateUnityAdsAsset_Validator()
		{
			var asset = Resources.Load<UnityAdsBundleDefinition>("Mediation/UnityAds/UnityAdsFyberBundle");
			return asset == null;
		}
#endif

	}
}

