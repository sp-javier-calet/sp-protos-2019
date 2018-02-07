using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimelinePlayables
{
    public class ScalePlayableMixer : BaseTransformPlayableMixer
    {
        Transform _trackBinding;
        Vector3 _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.localScale = _defaultValue;
                _firstFrameHappened = false;
            }
        }

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _trackBinding = playerData as Transform;
            if(_trackBinding == null)
            {
                return;
            }

            if(!_firstFrameHappened)
            {
                _defaultValue = _trackBinding.localScale;
                _firstFrameHappened = true;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var newScale = _defaultValue;
            var playTime = playable.GetGraph().GetRootPlayable(0).GetTime();

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTransformPlayableData>)playable.GetInput(i);
                var playableInputData = (ScalePlayableData)playableInput.GetBehaviour();
                playableInputData.ComputeAnimatedValues(_defaultValue);

                if(playTime - playableInputData.CustomClipEnd >= 0)
                {
                    newScale = playableInputData.AnimateTo;
                }
                else
                {
                    var tweenProgress = GetTweenProgress(playableInput, playableInputData, playTime);
                    if(tweenProgress > 0f)
                    {
                        newScale = Vector3.Lerp(playableInputData.AnimateFrom, playableInputData.AnimateTo, tweenProgress);
                    }
                }
            }

            _trackBinding.localScale = newScale;
        }
    }
}