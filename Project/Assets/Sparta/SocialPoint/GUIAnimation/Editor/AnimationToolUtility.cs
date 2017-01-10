using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Class with some helper methods used by animation tool gui components
    public static class AnimationToolUtility
    {
        public enum TextStyle
        {
            Title,
            Subtitle1,
            Subtitle2,
            Subtitle3,
            Text
        }

        public static List<string> ComponentsToNames<T>(List<T> AllAnimationsRoots) where T:Component
        {
            var _animationsRootsList = new List<string>();
            for(int i = 0; i < AllAnimationsRoots.Count; ++i)
            {
                _animationsRootsList.Add(i + " - " + AllAnimationsRoots[i].name);
            }

            return _animationsRootsList;
        }

        public static GUIStyle GetStyle(TextStyle textStyle, GUIStyle defaultStyle, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var style = new GUIStyle(defaultStyle);

            // Input
            style.alignment = anchor;

            // Common
            switch(textStyle)
            {
            case TextStyle.Title:
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;
                break;

            case TextStyle.Subtitle1:
                style.fontSize = 12;
                style.fontStyle = FontStyle.Normal;
                break;

            case TextStyle.Subtitle2:
                style.fontSize = 11;
                style.fontStyle = FontStyle.Bold;
                break;

            case TextStyle.Subtitle3:
                style.fontSize = 11;
                style.fontStyle = FontStyle.Normal;
                break;

            case TextStyle.Text:
                style.fontSize = 10;
                style.fontStyle = FontStyle.Normal;
                break;
            }
            return style;
        }
    }
}
