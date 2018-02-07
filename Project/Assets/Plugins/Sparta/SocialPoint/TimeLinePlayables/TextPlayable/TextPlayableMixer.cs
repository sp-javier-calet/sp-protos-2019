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
        string _defaultKey;
        string[] _defaultParams;
        SPText.TextEffect _defaultEffect;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.color = _defaultColor;
                _trackBinding.fontSize = _defaultFontSize;

                SetDefaultTextInfo();

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
                _defaultKey = _trackBinding.Key;
                _defaultParams = _trackBinding.Parameters;
                _defaultEffect = _trackBinding.Effect;
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

            var colorChanged = false;
            var textSizeChanged = false;
            var playTime = playable.GetGraph().GetRootPlayable(0).GetTime();
            TextPlayableData previousPlayableInputData = null;

            for(int i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                totalWeight += inputWeight;

                var playableInput = (ScriptPlayable<BasePlayableData>)playable.GetInput(i);
                var playableInputData = (TextPlayableData)playableInput.GetBehaviour();

                if(playableInputData.ChangeColor)
                {
                    colorChanged = true;
                    blendedColor += playableInputData.Color * inputWeight;
                }

                if(playableInputData.ChangeFontSize)
                {
                    textSizeChanged = true;
                    blendedFontSize += playableInputData.FontSize * inputWeight;
                }

                if(playableInputData.ChangeText)
                {
                    if(playTime - playableInputData.CustomClipStart >= 0 && !playableInputData.IsTextChanged)
                    {
                        playableInputData.IsTextChanged = true;
                        SetPlayableTextInfo(playableInputData);
                    }
                    else if(playTime - playableInputData.CustomClipStart < 0 && playableInputData.IsTextChanged)
                    {
                        playableInputData.IsTextChanged = false;

                        if(previousPlayableInputData == null)
                        {
                            SetDefaultTextInfo();
                        }
                        else
                        {
                            SetPlayableTextInfo(previousPlayableInputData);
                        }
                    }
                }

                // We want to store the previous text info
                previousPlayableInputData = playableInputData;
            }

            if(colorChanged)
            {
                _trackBinding.color = blendedColor + _defaultColor * (1f - totalWeight);
            }

            if(textSizeChanged)
            {
                _trackBinding.fontSize = Mathf.RoundToInt(blendedFontSize + _defaultFontSize * (1f - totalWeight));
            }
        }

        void SetPlayableTextInfo(TextPlayableData playableInputData)
        {
            SetTextInfo(playableInputData.Text, playableInputData.UseLocalizedData, playableInputData.Text, playableInputData.Params, playableInputData.Effect);
        }

        void SetDefaultTextInfo()
        {
            SetTextInfo(_defaultText, !string.IsNullOrEmpty(_defaultKey), _defaultKey, _defaultParams, _defaultEffect);
        }

        void SetTextInfo(string text, bool useLocalizedData, string key, string[] parameters, SPText.TextEffect effect)
        {
            if(useLocalizedData && !string.IsNullOrEmpty(key))
            {
                _trackBinding.SetKey(key, parameters, effect);
            }
            else
            {
                _trackBinding.text = text;
            }
        }
    }
}
