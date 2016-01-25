using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public interface IBlendeableEffect
	{
		bool UseEaseCustom { get; set; }
		List<Vector2> EaseCustom { get; set; }
		EaseType EaseType { get; set; }
	}
}
