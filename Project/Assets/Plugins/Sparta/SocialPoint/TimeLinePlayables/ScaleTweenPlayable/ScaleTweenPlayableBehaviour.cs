using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class ScaleTweenPlayableBehaviour : BaseTweenPlayableBehaviour
    {
        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);

            if(AnimPositionType == HowToAnimateType.UseReferencedTransforms)
            {
                if(TransformFrom != null)
                {
                    AnimateFrom = TransformFrom.position;
                }

                if(TransformTo != null)
                {
                    AnimateTo = TransformTo.position;
                }
            }
        }
    }
}
