using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Class to encapsulate the Mouse states
    public sealed class MouseController
    {
        MouseDoubleClickMonitor _doubleClick = new MouseDoubleClickMonitor();
        readonly Dictionary<int, bool> _buttons = new Dictionary<int, bool>();

        public void UpdateState()
        {
            _doubleClick.UpdateState();

            if(Event.current.type == EventType.MouseDown)
            {
                SetMouseButton(Event.current.button, true);
            }
            else if(Event.current.type == EventType.KeyUp)
            {
                SetMouseButton(Event.current.button, false);
            }
        }

        public bool IsDoubleClick()
        {
            return _doubleClick.DoubleClick;
        }

        public bool IsPressed(int code)
        {
            return _buttons.ContainsKey(code) && _buttons[code];
        }

        public void ResetState()
        {
            _buttons.Clear();
        }

        void SetMouseButton(int key, bool isEnabled)
        {
            if(_buttons.ContainsKey(key))
            {
                _buttons[key] = isEnabled;
            }
            else
            {
                _buttons.Add(key, isEnabled);
            }
        }
    }
}
