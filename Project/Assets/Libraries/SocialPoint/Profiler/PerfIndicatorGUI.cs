using UnityEngine;

namespace SocialPoint.Profiler
{
    public class PerfIndicatorGUI : MonoBehaviour
    {
        public PerfInfo Info;

        public TextAnchor Anchor; 
        public Vector2 Border = new Vector2(5.0f, 5.0f);
        public Color Color = Color.green;
        public bool ShortText = false;

        void Start()
        {
            if(Info == null)
            {
                Info = new PerfInfo(this);
            }
        }

        void OnGUI()
        {
            var rect = new Rect(Border.x, Border.y,
                Screen.width-2*Border.x, Screen.height-2*Border.x);

            var style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color;
            style.alignment = Anchor;

            string text;
            if(ShortText)
            {
                text = Info.ToShortString();
            }
            else
            {
                text = Info.ToString();
            }
            UnityEngine.GUI.BeginGroup(rect);
            UnityEngine.GUI.Label(new Rect(0.0f, 0.0f, rect.width, rect.height), text, style);
            UnityEngine.GUI.EndGroup();
        }
    }
}
