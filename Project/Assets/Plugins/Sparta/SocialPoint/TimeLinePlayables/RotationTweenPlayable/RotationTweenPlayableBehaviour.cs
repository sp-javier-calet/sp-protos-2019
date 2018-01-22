using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class RotationTweenPlayableBehaviour : BaseTweenPlayableBehaviour
    {
//        public Quaternion AnimateFrom;
//        public Quaternion AnimateTo;

        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);

            if(AnimPositionType == HowToAnimateType.UseReferencedTransforms)
            {
                if(TransformFrom != null)
                {
                    AnimateFrom = TransformFrom.rotation.eulerAngles;
                }

                if(TransformTo != null)
                {
                    AnimateTo = TransformTo.rotation.eulerAngles;
                }
            }
        }
    }
}