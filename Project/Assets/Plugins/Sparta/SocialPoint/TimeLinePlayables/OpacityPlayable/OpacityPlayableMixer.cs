using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SocialPoint.TimeLinePlayables
{
    public class OpacityPlayableMixer : BasePlayableMixer
    {
        public MaskableGraphic _trackBinding;
        public Color _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.color = _defaultValue;
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

            var blendedAlpha = 0f;
            var alphaTotalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BasePlayableData>)playable.GetInput(i);
                var playableBehaviour = (OpacityPlayableData)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);

                blendedAlpha += playableBehaviour.Alpha * inputWeight;
                alphaTotalWeight += inputWeight;
            }

            var color = _defaultValue;
            color.a = blendedAlpha + _defaultValue.a * (1f - alphaTotalWeight);

            _trackBinding.color = color;
        }
    }
}