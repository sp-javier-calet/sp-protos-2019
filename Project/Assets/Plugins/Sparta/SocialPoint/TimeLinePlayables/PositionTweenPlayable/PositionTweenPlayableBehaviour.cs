using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class PositionTweenPlayableBehaviour : BaseTweenPlayableBehaviour
    {
        public enum AnimatePositionType
        {
            UseAbsoluteWorldPosition,
            UseReferencedTransforms
        }

        public AnimatePositionType AnimPositionType;
        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;
        public Transform StartLocation;
        public Transform EndLocation;

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);

            if(AnimPositionType == AnimatePositionType.UseReferencedTransforms && StartLocation != null)
            {
                AnimateFrom = StartLocation.position;
            }
        }
    }
}