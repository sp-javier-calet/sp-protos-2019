using UnityEditor;
using UnityEngine;
using System;

namespace FyberEditor
{

	[Serializable]
	[BundleDefinitionAttribute("AdColony", "com.fyber.mediation.adcolony.AdColonyMediationAdapter", "2.3.3-r1", 3)]
	public class AdColonyBundleDefinition : BundleDefinition
	{
		
		[SerializeField]
		[FyberPropertyAttribute("app.id")]
		private string appId;

		[SerializeField]
		[FyberPropertyAttribute("zone.ids.rewarded.video")]
		private string zoneIdsRewardedVideo;

		[SerializeField]
		[FyberPropertyAttribute("zone.ids.interstitial")]
		private string zoneIdsInterstitial;

		[SerializeField]
		[FyberPropertyAttribute("client.options")]
		private string clientOptions;

	}

}
