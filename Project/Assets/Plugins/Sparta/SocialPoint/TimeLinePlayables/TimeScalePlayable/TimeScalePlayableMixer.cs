using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class TimeScalePlayableMixer : BasePlayableMixer
    {
        float _defaultTimeScale = 1f;

        public override void OnGraphStart(Playable playable)
        {
            _defaultTimeScale = Time.timeScale;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var blendedTimeScale = 0f;
            var totalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);

                totalWeight += inputWeight;

                var playableInput = (ScriptPlayable<TimeScalePlayableData>)playable.GetInput(i);
                var playableBehaviour = playableInput.GetBehaviour();

                blendedTimeScale += inputWeight * playableBehaviour.TimeScale;
            }

            Time.timeScale = blendedTimeScale + _defaultTimeScale * (1f - totalWeight);
        }

        public override void OnGraphStop(Playable playable)
        {
            Time.timeScale = _defaultTimeScale;
        }
    }
}
