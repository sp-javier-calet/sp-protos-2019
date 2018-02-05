using System;
using SocialPoint.Base;
using SocialPoint.Pooling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class DestroyPlayableData : BasePlayableData
    {
        public GameObject GameObject;
        public bool InitialActiveState;
        public bool UsePooling;
        public bool IsDestroyed;

        public TimelineClip CustomClipReference { get; set; }
        public double CustomClipStart
        {
            get
            {
                return CustomClipReference.start;
            }
        }

        public double CustomClipEnd
        {
            get
            {
                return CustomClipReference.end;
            }
        }

        public override void OnGraphStop(Playable playable)
        {
            if(!Application.isPlaying)
            {
                SetActiveState(InitialActiveState);
            }
        }

        public void SetActiveState(bool active)
        {
            if(GameObject != null)
            {
                if(!Application.isPlaying)
                {
                    if(!active)
                    {
                        DestroyGameObject();
                    }
                }
                else
                {
                    GameObject.SetActive(active);
                }
            }
        }

        void DestroyGameObject()
        {
            if(UsePooling)
            {
                GameObject.Recycle();
            }
            else
            {
                GameObject.DestroyAnyway();
            }
        }
    }
}