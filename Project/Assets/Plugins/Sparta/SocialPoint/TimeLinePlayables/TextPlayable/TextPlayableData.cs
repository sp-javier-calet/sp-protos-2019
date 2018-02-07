using System;
using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable, NotKeyable]
    public class TextPlayableData : BaseTransformPlayableData
    {
        public bool ChangeColor;
        public bool ChangeFontSize;
        public bool ChangeText;
        public Color Color = Color.white;
        public int FontSize = 14;
        public string Text;
        public bool UseLocalizedData;
        public SPText.TextEffect Effect;
        public string[] Params;
        public bool IsTextChanged;
    }
}