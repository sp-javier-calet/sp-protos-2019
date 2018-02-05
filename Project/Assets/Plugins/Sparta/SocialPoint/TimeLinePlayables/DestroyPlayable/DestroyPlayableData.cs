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
            SetActiveState(InitialActiveState);
        }

        public void SetActiveState(bool active)
        {
            if(GameObject != null)
            {
#if UNITY_EDITOR
                GameObject.SetActive(active);
#else
                if(!active)
                {
                    DestroyGameObject();
                }
#endif
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