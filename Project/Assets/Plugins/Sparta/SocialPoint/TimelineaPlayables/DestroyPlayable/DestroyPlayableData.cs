using System;
using SocialPoint.Base;
using SocialPoint.Pooling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class DestroyPlayableData : BasePlayableData
    {
        public GameObject GameObject;
        public bool InitialActiveState;
        public bool UsePooling;
        public bool IsDestroyed;

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
                GameObject.SetActive(active);
            }
        }

        public void DestroyGameObject()
        {
            if(GameObject != null)
            {
                if(Application.isPlaying && UsePooling)
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
}