using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class ScaleTweenPlayableBehaviour : BaseTweenPlayableBehaviour
    {
        public bool UseCurrentFromValue = true;
        public bool UseCurrentToValue;

        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);

            if(HowToAnimate == HowToAnimateType.UseReferencedTransforms)
            {
                if(TransformFrom != null)
                {
                    AnimateFrom = TransformFrom.position;
                }
                else
                {
                    throw new UnityException("If you use referenced transforms, you need to specify a 'From Transfom'");
                }

                if(TransformTo != null)
                {
                    AnimateTo = TransformTo.position;
                }
                else
                {
                    throw new UnityException("If you use referenced transforms, you need to specify a 'To Transfom'");
                }
            }
        }
    }
}
