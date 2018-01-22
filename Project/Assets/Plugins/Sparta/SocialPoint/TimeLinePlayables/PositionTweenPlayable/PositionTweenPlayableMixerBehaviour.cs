using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class PositionTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Transform trackBinding = playerData as Transform;

            if(trackBinding == null)
            {
                return;
            }

            var defaultPosition = trackBinding.position;

            // Get number of tracks for the clip
            var inputCount = playable.GetInputCount();
            var positionTotalWeight = 0f;
            var blendedPosition = Vector3.zero;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTweenPlayableBehaviour>)playable.GetInput(i);
                var playableBehaviour = (PositionTweenPlayableBehaviour)playableInput.GetBehaviour();

//                if(playableBehaviour.AnimPositionType == PositionTweenPlayableBehaviour.HowToAnimateType.UseReferencedTransforms)
//                {
//                    if(playableBehaviour.TransformTo == null)
//                    {
//                        continue;
//                    }
//
//                    playableBehaviour.AnimateTo = playableBehaviour.TransformTo.position;
//                }

                if(trackBinding.position == playableBehaviour.AnimateTo)
                {
                    continue;
                }

                var inputWeight = playable.GetInputWeight(i);

                if(!_firstFrameHappened && playableBehaviour.AnimPositionType == BaseTweenPlayableBehaviour.HowToAnimateType.UseReferencedTransforms && playableBehaviour.TransformFrom == null)
                {
                    playableBehaviour.AnimateFrom = defaultPosition;
                    _firstFrameHappened = true;
                }

                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                positionTotalWeight += inputWeight;
                blendedPosition += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }
                
            trackBinding.position = blendedPosition + defaultPosition * (1f - positionTotalWeight);
        }
    }
}