using SocialPoint.Utils;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class BaseTransformPlayableMixer : BasePlayableMixer
    {
        protected static float GetTweenProgress(ScriptPlayable<BaseTransformPlayableData> playableInput, BaseTransformPlayableData playableBehaviour)
        {
            var time = playableInput.GetTime();
            var normalisedTime = (float)(time * playableBehaviour.InverseDuration);

            if(playableBehaviour.AnimationType == BaseTransformPlayableData.AnimateType.AnimationCurve && playableBehaviour.AnimationCurve != null)
            {
                return playableBehaviour.AnimationCurve.Evaluate(normalisedTime);
            }
            else
            {
               return playableBehaviour.EaseType.ToFunction()(normalisedTime, 0f, 1f, 1f);
            }
        }
    }
}