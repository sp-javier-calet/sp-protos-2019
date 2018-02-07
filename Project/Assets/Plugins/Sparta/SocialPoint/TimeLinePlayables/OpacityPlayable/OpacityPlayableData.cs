using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class OpacityPlayableData : BasePlayableData
    {
        [Range(0f, 1f)]
        public float Alpha;
    }
}