using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SocialPoint.TimelinePlayables
{
    public class ColorPlayableMixer : BasePlayableMixer
    {
        MaskableGraphic _trackBinding;
        Color _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.color = _defaultValue;
                _firstFrameHappened = false;
            }
        }

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _trackBinding = playerData as MaskableGraphic;
            if(_trackBinding == null)
            {
                return;
            }

            if(!_firstFrameHappened)
            {
                _defaultValue = _trackBinding.color;

                _firstFrameHappened = true;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var blendedColor = Color.clear;
            var colorTotalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BasePlayableData>)playable.GetInput(i);
                var playableBehaviour = (ColorPlayableData)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);

                blendedColor += playableBehaviour.Color * inputWeight;
                colorTotalWeight += inputWeight;
            }

            _trackBinding.color = blendedColor + _defaultValue * (1f - colorTotalWeight);
        }
    }
}