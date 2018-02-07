using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimelinePlayables
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
                var playableInputData = playableInput.GetBehaviour();

                if(playTime - playableInputData.CustomClipStart >= 0 && !playableInputData.IsDestroyed)
                {
                    playableInputData.IsDestroyed = true;

                    if(!Application.isPlaying)
                    {
                        playableInputData.SetActiveState(false);
                    }
                    else
                    {
                        playableInputData.DestroyGameObject();
                    }
                }
                else if(!Application.isPlaying && playTime - playableInputData.CustomClipStart < 0 && playableInputData.IsDestroyed)
                {
                    playableInputData.IsDestroyed = false;
                    playableInputData.SetActiveState(playableInputData.InitialActiveState);
                }
            }
        }
    }
}
