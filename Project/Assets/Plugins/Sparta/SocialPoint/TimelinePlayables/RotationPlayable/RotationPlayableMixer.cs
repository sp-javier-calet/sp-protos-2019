using SocialPoint.Base;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimelinePlayables
{
    public class RotationPlayableMixer : BaseTransformPlayableMixer
    {
        Transform _trackBinding;
        Quaternion _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.rotation = _defaultValue;
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
                _defaultValue = _trackBinding.rotation;
                _firstFrameHappened = true;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var newRotation = _defaultValue;
            var playTime = playable.GetGraph().GetRootPlayable(0).GetTime();

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTransformPlayableData>)playable.GetInput(i);
                var playableInputData = (RotationPlayableData)playableInput.GetBehaviour();
                playableInputData.ComputeAnimatedValues(_defaultValue);

                if(playTime - playableInputData.CustomClipEnd >= 0)
                {
                    newRotation = playableInputData.AnimateTo;
                }
                else
                {
                    var tweenProgress = GetTweenProgress(playableInput, playableInputData, playTime);
                    if(tweenProgress > 0f)
                    {
                        newRotation = Quaternion.Lerp(playableInputData.AnimateFrom, playableInputData.AnimateTo, tweenProgress);
                    }
                }
            }

            _trackBinding.rotation = newRotation;
        }
    }
}