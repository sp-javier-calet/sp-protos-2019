using UnityEngine;

namespace SocialPoint.Profiling
{
    public sealed class PerfInfoGUI : MonoBehaviour
    {
        public PerfInfo Info;

        public TextAnchor Anchor;
        public Vector2 Border = new Vector2(5.0f, 5.0f);
        public Color Color = Color.green;
        public bool ShortText;
        public bool StartEnabled;

        const float defaultSize = 20;
        const float defaultWidth = 1280;

        void Start()
        {
            if(Info == null)
            {
                Info = new PerfInfo(this);
            }

            enabled = StartEnabled;
        }

        void OnGUI()
        {
            var rect = new Rect(Border.x, Border.y,
                           Screen.width - 2 * Border.x, Screen.height - 2 * Border.x);

            var style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color;
            style.alignment = Anchor;
            style.fontSize = Mathf.RoundToInt(defaultSize * Screen.width / (defaultWidth * 1.0f));

            string text;
            text = ShortText ? Info.ToShortString() : Info.ToString();
            GUI.BeginGroup(rect);
            GUI.Label(new Rect(0.0f, 0.0f, rect.width, rect.height), text, style);
            GUI.EndGroup();
        }
    }
}
