using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class ColorPlayableData : BasePlayableData
    {
        public Color Color;
    }
}