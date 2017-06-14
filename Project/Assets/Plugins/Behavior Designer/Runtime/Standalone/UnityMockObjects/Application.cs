using System;

namespace BehaviorDesigner.Runtime.Standalone
{   
    public static class Application
    {
        public static Func<bool> IsPlayingDelegate = null;

        public static bool isPlaying { get{ return IsPlayingDelegate != null ? IsPlayingDelegate() : true; } }
    }
}