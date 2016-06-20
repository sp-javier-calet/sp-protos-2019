using UnityEditor;
using UnityEngine;
using System;

namespace FyberEditor
{

	[Serializable]
	[BundleDefinitionAttribute("Chartboost", "com.fyber.mediation.chartboost.ChartboostMediationAdapter", "6.4.1-r1", 3)]
	public class ChartboostBundleDefinition : BundleDefinition
	{

		[SerializeField]
		[FyberPropertyAttribute("AppId")]
		private string appId;

		[SerializeField]
		[FyberPropertyAttribute("AppSignature")]
		private string appSignature;

		[SerializeField]
		[FyberPropertyAttribute("LogLevel")]
		private string logLevel;

		[SerializeField]
		[FyberPropertyAttribute("CacheRewardedVideo")]
		private bool cacheInterstitials;

		[SerializeField]
		[FyberPropertyAttribute("CacheInterstitials")]
		private bool cacheRewardedVideo;

	}

}
