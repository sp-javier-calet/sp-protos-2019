using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable, NotKeyable]
    public class OpacityPlayableData : BasePlayableData
    {
        [Range(0f, 1f)]
        public float Alpha;
    }
}