using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class RotationTweenPlayableBehaviour : BaseTweenPlayableBehaviour
    {
        public bool UseCurrentFromValue = true;
        public bool UseCurrentToValue;

        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);

            if(AnimPositionType == HowToAnimateType.UseReferencedTransforms)
            {
                if(TransformFrom != null)
                {
                    AnimateFrom = TransformFrom.eulerAngles;
                }
                else
                {
                    throw new UnityException("If you use referenced transforms, you need to specify a 'From Transfom'");
                }

                if(TransformTo != null)
                {
                    AnimateTo = TransformTo.eulerAngles;
                }
                else
                {
                    throw new UnityException("If you use referenced transforms, you need to specify a 'To Transfom'");
                }
            }
        }
    }
}