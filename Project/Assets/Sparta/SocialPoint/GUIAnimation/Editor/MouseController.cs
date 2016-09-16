using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
    // Class to encapsulate the Mouse states
    public sealed class MouseController
    {
        MouseDoubleClickMonitor _doubleClick = new MouseDoubleClickMonitor();
        Dictionary<int, bool> _buttons = new Dictionary<int, bool>();

        public void UpdateState()
        {
            _doubleClick.UpdateState();

            if(Event.current.type == EventType.mouseDown)
            {
                SetMouseButton(Event.current.button, true);
            }
            else if(Event.current.type == EventType.keyUp)
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
            if(!_buttons.ContainsKey(code))
            {
                return false;
            }
            else
            {
                return _buttons[code];
            }
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
