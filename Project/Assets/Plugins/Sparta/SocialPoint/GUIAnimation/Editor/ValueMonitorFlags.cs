using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
    // Class used to store a boolean, for instance when some scene object changes we can set the flag FlagType.ObjectChanged=true
    //, then we can use this flag anywhere in the GUI to do something, for instance, draw with red color a button that say "Save Object New State"
    sealed class ValueMonitorFlags<T> where T : System.IComparable
    {
        public Dictionary<T, bool> _eventValues = new Dictionary<T, bool>();

        public void Init()
        {
            ResetState();
        }

        public void ResetState()
        {
            _eventValues.Clear();
        }

        public void SetFlag(T flag, bool value)
        {
            if(!_eventValues.ContainsKey(flag))
            {
                _eventValues.Add(flag, false);
            }
            _eventValues[flag] = value;
        }

        public bool GetFlag(T flag)
        {
            bool value;
            _eventValues.TryGetValue(flag, out value);
            return value;
        }

        public bool ResetFlag(T flag)
        {
            bool value;
            if(_eventValues.TryGetValue(flag, out value))
            {
                _eventValues[flag] = false;
            }
            return value;
        }
    }
}
