using System;
using UnityEngine;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class OpacityPlayableData : BasePlayableData
    {
        [Range(0f, 1f)]
        public float Alpha;
    }
}