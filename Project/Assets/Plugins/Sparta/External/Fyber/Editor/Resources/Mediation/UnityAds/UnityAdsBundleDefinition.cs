using UnityEngine;
using UnityEditor;
using System;

namespace FyberEditor
{
	
	[Serializable]
	[BundleDefinitionAttribute("UnityAds", "com.fyber.mediation.unityads.UnityAdsMediationAdapter", "1.5.6-r2", 3, InternalName="Applifier")]
	public class UnityAdsBundleDefinition : BundleDefinition
	{

		[SerializeField]
		[FyberPropertyAttribute("game.id.key")]
		private string gameIdKey;
	
		[SerializeField]
		[FyberPropertyAttribute("debug.mode")]
		private bool debugMode;

		[SerializeField]
		[FyberPropertyAttribute("zone.id.interstitial")]
		private string zoneIdInterstitial;

		[SerializeField]
		[FyberPropertyAttribute("zone.id.rewarded.video")]
		private string zoneIdRewardedVideo;

	}
}
