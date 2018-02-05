using System;
using UnityEngine;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class RotationPlayableData : BaseTransformPlayableData
    {
        public Quaternion AnimateFrom;
        public Quaternion AnimateTo;

        public void SetAnimatedValues(Quaternion defaultValue)
        {
            if(HowToAnimateFrom == HowToAnimateType.UseReferenceTransform)
            {
                if(TransformFrom != null)
                {
                    AnimateFrom = TransformFrom.rotation;
                }
                else
                {
                    throw new UnityException("If you use referenced transforms, you need to specify a 'From Transfom'");
                }
            }
            else
            {
                if(HowToAnimateFrom == HowToAnimateType.UseInitialTransformValues)
                {
                    AnimateFrom = defaultValue;
                }

                TransformFrom = null;
            }

            if(HowToAnimateTo == HowToAnimateType.UseReferenceTransform)
            {
                if(TransformTo != null)
                {
                    AnimateTo = TransformTo.rotation;
                }
                else
                {
                    throw new UnityException("If you use referenced transforms, you need to specify a 'To Transfom'");
                }
            }
            else
            {
                if(HowToAnimateTo == HowToAnimateType.UseInitialTransformValues)
                {
                    AnimateTo = defaultValue;
                }

                TransformTo = null;
            }
        }
    }
}