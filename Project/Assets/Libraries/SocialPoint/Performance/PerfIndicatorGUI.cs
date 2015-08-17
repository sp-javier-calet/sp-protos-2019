using UnityEngine;

namespace SocialPoint.Performance
{
    public class PerfIndicatorGUI : MonoBehaviour
    {
        public PerfStats Stats;

        public Vector2 Border = new Vector2(5.0f, 5.0f);
        public Rect ScreenRect = new Rect(1.0f, 1.0f, 150f, 45f);
        public Color Color = Color.green;

        const string TextFormat = @"FPS: {0:F0}
Tris: {1} ({2} batched)
Drawcalls: {3} ({4} batched)";

        public string Text
        {
            get
            {
                return string.Format(TextFormat,
                    Stats.Frame.FPS,
                    Stats.Frame.Tris, Stats.Frame.BatchedTris,
                    Stats.Frame.DrawCalls, Stats.Frame.BatchedDrawCalls);
            }
        }

        void Start()
        {
            if(Stats == null)
            {
                Stats = new PerfStats(this);
            }
        }

        void OnGUI()
        {
            var rect = new Rect((Screen.width-2*Border.x)*ScreenRect.x,
                                (Screen.height-2*Border.y)*ScreenRect.y,
                                ScreenRect.width, ScreenRect.width);

            var style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color;

            if(ScreenRect.x > 0.5f)
            {
                rect.x -= ScreenRect.width;
                if(ScreenRect.y > 0.5f)
                {
                    rect.y -= ScreenRect.height;
                    style.alignment = TextAnchor.LowerRight;
                }
                else
                {
                    style.alignment = TextAnchor.UpperRight;
                }
            }
            else
            {
                if(ScreenRect.y > 0.5f)
                {
                    rect.y -= ScreenRect.height;
                    style.alignment = TextAnchor.LowerLeft;
                }
                else
                {
                    style.alignment = TextAnchor.UpperLeft;
                }
            }
            UnityEngine.GUI.BeginGroup(rect);
            UnityEngine.GUI.Label(new Rect(0.0f, 0.0f, ScreenRect.width, ScreenRect.height), Text, style);
            UnityEngine.GUI.EndGroup();
        }
    }
}
