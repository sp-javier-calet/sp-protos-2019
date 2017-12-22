using UnityEditor;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Class to detect doble clicks
    public sealed class MouseDoubleClickMonitor
    {
        const float kMaxTimeToDoubleClick = 0.50f;

        double _lastTimeClicked;
        bool _doubleClick;

        public bool DoubleClick
        {
            get
            {
                return _doubleClick;
            } 
        }

        public void UpdateState()
        {
            if(Event.current.type == EventType.MouseDown)
            {
                _doubleClick = IsDoubleClick();
                _lastTimeClicked = EditorApplication.timeSinceStartup;
            }
        }

        bool IsDoubleClick()
        {
            return (Abs(_lastTimeClicked - EditorApplication.timeSinceStartup) < kMaxTimeToDoubleClick);
        }

        static double Abs(double val)
        {
            return val < 0 ? -val : val;
        }
    }
}
