using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable, NotKeyable]
    public class TextPlayableData : BaseTransformPlayableData
    {
        public Color Color = Color.white;
        public int FontSize = 14;
        public string Text;
        public bool UseLocalizedData;
    }
}