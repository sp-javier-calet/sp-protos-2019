using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class DestroyPlayableMixer : BasePlayableMixer
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var playTime = playable.GetGraph().GetRootPlayable(0).GetTime();
            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<DestroyPlayableData>)playable.GetInput(i);
                var playableBehaviour = playableInput.GetBehaviour();

                if(playTime - playableBehaviour.CustomClipStart >= 0 && !playableBehaviour.IsDestroyed)
                {
                    playableBehaviour.IsDestroyed = true;

                    if(!Application.isPlaying)
                    {
                        playableBehaviour.SetActiveState(false);
                    }
                    else
                    {
                        playableBehaviour.DestroyGameObject();
                    }
                }
                else if(!Application.isPlaying && playTime - playableBehaviour.CustomClipStart < 0 && playableBehaviour.IsDestroyed)
                {
                    playableBehaviour.IsDestroyed = false;
                    playableBehaviour.SetActiveState(playableBehaviour.InitialActiveState);
                }
            }
        }
    }
}
