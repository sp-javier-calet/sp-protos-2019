using UnityEngine;
using UnityEditor;

namespace SocialPoint.GUIAnimation
{
	// Class to detect doble clicks
	public sealed class MouseDoubleClickMonitor 
	{
		const float kMaxTimeToDoubleClick = 0.50f;

		double _lastTimeClicked = 0;
		bool _doubleClick = false;
		public bool DoubleClick 
		{
			get
			{
				return _doubleClick;
			} 
		} 

		public void UpdateState()
		{
			if(Event.current.type == EventType.mouseDown)
			{
				_doubleClick = IsDoubleClick();
				_lastTimeClicked = EditorApplication.timeSinceStartup;
			}
		}

		bool IsDoubleClick()
		{
			return (Abs(_lastTimeClicked - EditorApplication.timeSinceStartup) < kMaxTimeToDoubleClick);
		}

		double Abs(double val)
		{
			return val < 0 ? -val : val;
		}
	}
}
