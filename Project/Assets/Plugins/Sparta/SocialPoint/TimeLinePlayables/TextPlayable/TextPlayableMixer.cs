using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class TextPlayableMixer : BasePlayableMixer
    {
        SPText _trackBinding;
        Color _defaultColor;
        int _defaultFontSize;
        string _defaultText;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.color = _defaultColor;
                _trackBinding.fontSize = _defaultFontSize;
                _trackBinding.text = _defaultText;
                _firstFrameHappened = false;
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _trackBinding = playerData as SPText;
            if(_trackBinding == null)
            {
                return;
            }

            if(!_firstFrameHappened)
            {
                _defaultColor = _trackBinding.color;
                _defaultFontSize = _trackBinding.fontSize;
                _defaultText = _trackBinding.text;
                _firstFrameHappened = true;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var blendedColor = Color.clear;
            var blendedFontSize = 0f;
            var totalWeight = 0f;
            var greatestWeight = 0f;
            var currentInputs = 0;

            for(int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                var playableInput = (ScriptPlayable<BasePlayableData>)playable.GetInput(i);
                var playableInputData = (TextPlayableData)playableInput.GetBehaviour();

                blendedColor += playableInputData.Color * inputWeight;
                blendedFontSize += playableInputData.FontSize * inputWeight;

                totalWeight += inputWeight;
                if(inputWeight > greatestWeight)
                {
                    _trackBinding.text = playableInputData.Text;
                    greatestWeight = inputWeight;
                }

                if(!Mathf.Approximately(inputWeight, 0f))
                {
                    currentInputs++;
                }
            }

            _trackBinding.color = blendedColor + _defaultColor * (1f - totalWeight);
            _trackBinding.fontSize = Mathf.RoundToInt(blendedFontSize + _defaultFontSize * (1f - totalWeight));

            if(currentInputs != 1 && 1f - totalWeight > greatestWeight)
            {
                _trackBinding.text = _defaultText;
            }
        }
    }
}
