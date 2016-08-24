using System;
using UnityEditor;
using UnityEngine;

namespace FyberEditor
{
	public class AdColonyAssetMenu
	{

#if !UNITY_5
		[MenuItem("Fyber/Mediation Assets Generation/AdColony")]
		public static void GenerateAdColonyAsset()
		{
			var asset = ScriptableObject.CreateInstance<AdColonyBundleDefinition>();
			AssetDatabase.CreateAsset(asset, "Assets/Fyber/Editor/Resources/Mediation/AdColony/AdColonyFyberBundle.asset");
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Fyber/Mediation Assets Generation/AdColony", true)]
		public static bool GenerateAdColonyAsset_Validator()
		{
			var asset = Resources.Load<AdColonyBundleDefinition>("Mediation/AdColony/AdColonyFyberBundle");
			return asset == null;
		}
#endif

	}
}

