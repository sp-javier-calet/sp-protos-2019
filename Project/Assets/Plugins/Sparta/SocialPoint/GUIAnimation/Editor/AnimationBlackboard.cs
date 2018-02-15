using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
    // Class to store string values shared between animation panels
    // Example: A panel can set the key: "FocusedPanel", then Panels can take that into account to react to
    // mouse click or any event only if they are the FocusedPanel
    public sealed class AnimationBlackboard
    {
        readonly Dictionary<string, string> _blackBoard = new Dictionary<string, string>();

        public void ResetState()
        {
            _blackBoard.Clear();
        }

        public bool CompareValue(string key, string value)
        {
            if(!_blackBoard.ContainsKey(key))
            {
                return false;
            }
            return _blackBoard[key] == value;
        }

        public void Set(string key, string value)
        {
            if(_blackBoard.ContainsKey(key))
            {
                _blackBoard[key] = value;
            }
            else
            {
                _blackBoard.Add(key, value);
            }
        }

        public bool Remove(string key)
        {
            return _blackBoard.Remove(key);
        }
    }
}
