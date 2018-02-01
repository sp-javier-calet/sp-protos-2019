using System;
using UnityEngine;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class ScalePlayableData : BaseTransformPlayableData
    {
        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;

        public void SetAnimatedValues(Vector3 defaultTransformValue)
        {
            if(HowToAnimateFrom == HowToAnimateType.UseReferenceTransform)
            {
                if(TransformFrom != null)
                {
                    AnimateFrom = TransformFrom.localScale;
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
                    AnimateFrom = defaultTransformValue;
                }

                TransformFrom = null;
            }

            if(HowToAnimateTo == HowToAnimateType.UseReferenceTransform)
            {
                if(TransformTo != null)
                {
                    AnimateTo = TransformTo.localScale;
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
                    AnimateTo = defaultTransformValue;
                }

                TransformTo = null;
            }
        }
    }
}
