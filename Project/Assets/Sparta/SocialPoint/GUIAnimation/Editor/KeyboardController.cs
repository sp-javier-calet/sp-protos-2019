using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Class that encapsulates the Keyboard state
    public sealed class KeyboardController
    {
        readonly Dictionary<KeyCode, bool> _keys = new Dictionary<KeyCode, bool>();

        public void UpdateState()
        {
            ResetState();

            if(Event.current.type == EventType.keyDown)
            {
                SetKeyValue(Event.current.keyCode, true);
            }

            SetSpecialControls();
        }

        void SetSpecialControls()
        {
            // Control
            if(Event.current.control)
            {
                SetKeyValue(KeyCode.LeftControl, true);
            }
            else
            {
                SetKeyValue(KeyCode.LeftControl, false);
            }

            // Shift
            if(Event.current.shift)
            {
                SetKeyValue(KeyCode.LeftShift, true);
            }
            else
            {
                SetKeyValue(KeyCode.LeftShift, false);
            }

            // Shift
            if(Event.current.alt)
            {
                SetKeyValue(KeyCode.LeftAlt, true);
            }
            else
            {
                SetKeyValue(KeyCode.LeftAlt, false);
            }

            // Shift
            if(Event.current.command)
            {
                SetKeyValue(KeyCode.LeftCommand, true);
            }
            else
            {
                SetKeyValue(KeyCode.LeftCommand, false);
            }

            // Enter
            if(Event.current.type == EventType.keyDown)
            {
                if(Event.current.character == '\n')
                {
                    SetKeyValue(KeyCode.LeftWindows, true);
                }
            }
        }

        public bool IsPressed(KeyCode code)
        {
            return _keys.ContainsKey(code) && _keys[code];
        }

        public void ResetState()
        {
            _keys.Clear();
        }

        void SetKeyValue(KeyCode key, bool isEnabled)
        {
            _keys.Remove(key);
            _keys.Add(key, isEnabled);
        }
    }
}
