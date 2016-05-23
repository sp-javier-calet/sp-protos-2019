using System;
using UnityEditor;
using UnityEngine;

namespace FyberEditor
{
	public class ChartboostAssetMenu
	{

#if !UNITY_5
		[MenuItem("Fyber/Mediation Assets Generation/Chartboost")]
		public static void GenerateChartboostAsset()
		{
			var asset = ScriptableObject.CreateInstance<ChartboostBundleDefinition>();
			AssetDatabase.CreateAsset(asset, "Assets/Fyber/Editor/Resources/Mediation/Chartboost/ChartboostFyberBundle.asset");
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Fyber/Mediation Assets Generation/Chartboost", true)]
		public static bool GenerateChartboostAsset_Validator()
		{
			var asset = Resources.Load<ChartboostBundleDefinition>("Mediation/Chartboost/ChartboostFyberBundle");
			return asset == null;
		}
#endif

	}
}

