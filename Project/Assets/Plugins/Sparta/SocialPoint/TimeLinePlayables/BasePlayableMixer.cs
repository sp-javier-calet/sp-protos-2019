using SocialPoint.Base;
using UnityEngine.Playables;

namespace SocialPoint.TimelinePlayables
{
    public class BasePlayableMixer : PlayableBehaviour
    {
        protected bool _firstFrameHappened;

        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        protected virtual void DebugLog(string msg)
        {
            Log.i(string.Format("PlayableMixerBehaviour msg {0}", msg));
        }
    }
}