using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SocialPoint.TimeLinePlayables
{
    public class OpacityPlayableMixer : BasePlayableMixer
    {
        CanvasGroup _trackBinding;
        float _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.alpha = _defaultValue;
                _firstFrameHappened = false;
            }
        }

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _trackBinding = playerData as CanvasGroup;
            if(_trackBinding == null)
            {
                return;
            }

            if(!_firstFrameHappened)
            {
                _defaultValue = _trackBinding.alpha;
                _firstFrameHappened = true;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var blendedAlpha = 0f;
            var alphaTotalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BasePlayableData>)playable.GetInput(i);
                var playableInputData = (OpacityPlayableData)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);

                blendedAlpha += playableInputData.Alpha * inputWeight;
                alphaTotalWeight += inputWeight;
            }

            _trackBinding.alpha = blendedAlpha + _defaultValue * (1f - alphaTotalWeight);
        }
    }
}