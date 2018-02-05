using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class InstantiatePlayableMixer : BasePlayableMixer
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
                var playableInput = (ScriptPlayable<InstantiatePlayableData>)playable.GetInput(i);
                var playableBehaviour = playableInput.GetBehaviour();

                if(playTime - playableBehaviour.CustomClipStart >= 0 && !playableBehaviour.IsInstantiated)
                {
                    playableBehaviour.IsInstantiated = true;
                    playableBehaviour.InstantiateOrSpawn();
                }
                else if(playTime - playableBehaviour.CustomClipStart < 0 && playableBehaviour.IsInstantiated)
                {
                    playableBehaviour.IsInstantiated = false;
                    playableBehaviour.DestroyOrRecycle();
                }
            }
        }
    }
}
