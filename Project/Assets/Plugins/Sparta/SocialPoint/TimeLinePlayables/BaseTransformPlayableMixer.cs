using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class BaseTransformPlayableMixer : BasePlayableMixer
    {
        protected static float GetTweenProgress(ScriptPlayable<BaseTransformPlayableData> playableInput, BaseTransformPlayableData playableInputBehaviour, double playTime)
        {
            var duration = playableInputBehaviour.CustomClipEnd - playableInputBehaviour.CustomClipStart;
            var normalizedClipPosition = Mathf.Clamp01((float)((playTime - playableInputBehaviour.CustomClipStart) / duration));

            if(playableInputBehaviour.AnimationType == BaseTransformPlayableData.AnimateType.AnimationCurve && playableInputBehaviour.AnimationCurve != null)
            {
                return playableInputBehaviour.AnimationCurve.Evaluate(normalizedClipPosition);
            }
            else
            {
                return playableInputBehaviour.EaseType.ToFunction()(normalizedClipPosition, 0f, 1f, 1f);
            }
        }
    }
}