using SocialPoint.Base;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    public class DestroyPlayableMixer : PrefabControlPlayable
    {
        //GameObject _gameObject;

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        //public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        //{
        //    // Get number of clips for the current track
        //    var inputCount = playable.GetInputCount();
        //    if(inputCount == 0)
        //    {
        //        return;
        //    }

        //    //float totalWeight = 0f;
        //    float greatestWeight = 0f;
        //    //int currentInputs = 0;

        //    for(int i = 0; i < inputCount; i++)
        //    {
        //        var playableInput = (ScriptPlayable<BasePlayableData>)playable.GetInput(i);
        //        var playableBehaviour = (DestroyPlayableData)playableInput.GetBehaviour();
        //        var inputWeight = playable.GetInputWeight(i);
        //        //totalWeight += inputWeight;

        //        if(inputWeight > greatestWeight)
        //        {
        //            Debug.Log("destroy gameobject " + playableBehaviour.GameObject);

        //            playableBehaviour.GameObject.DestroyAnyway();
        //            greatestWeight = inputWeight;
        //        }

        //        //if(!Mathf.Approximately(inputWeight, 0f))
        //        //{
        //        //    currentInputs++;
        //        //}

        //        //Debug.Log("inputWeight " +  inputWeight + " ");

        //        //blendedColor += playableBehaviour.Color * inputWeight;
        //        //colorTotalWeight += inputWeight;

        //        //if(inputWeight > colorGreatestWeight)
        //        //{
        //        //    colorGreatestWeight = inputWeight;
        //        //}

        //        //if(!Mathf.Approximately(inputWeight, 0f))
        //        //{
        //        //    currentInputs++;
        //        //}
        //    }

        //    //if(currentInputs != 1 && 1f - totalWeight > greatestWeight)
        //    //{
        //    //    m_TrackBinding.text = m_DefaultText;
        //    //}
        //}


        //public GameObject ObjectToDestroy;

        //public override void OnBehaviourPlay(Playable playable, FrameData info)
        //{
        //    OnPlayStateChanged(playable, playable.GetPlayState());
        //}

        //public override void OnBehaviourPause(Playable playable, FrameData info)
        //{
        //    OnPlayStateChanged(playable, playable.GetPlayState());
        //}

        //public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        //{
        //    base.ProcessFrame(playable, info, playerData);
        //}

        //private void OnPlayStateChanged(Playable playable, PlayState newState)
        //{
        //    if(newState == PlayState.Playing)
        //    {
        //        playable..GameObject.DestroyAnyway();
        //    }
        //}
    }
}
