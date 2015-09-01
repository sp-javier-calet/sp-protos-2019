using UnityEngine;
using SocialPoint.AdminPanel;

namespace SocialPoint.Profiler
{
    public class PerfInfoGUI : MonoBehaviour
    {
        public PerfInfo Info;

        public TextAnchor Anchor; 
        public Vector2 Border = new Vector2(5.0f, 5.0f);
        public Color Color = Color.green;
        public bool ShortText = false;

        public SocialPoint.AdminPanel.AdminPanel AdminPanel
        {
            set
            {
                value.AddPanelGUI("System", new PerfInfoAdminGUI(this));
            }
        }


        void Start()
        {
            if(Info == null)
            {
                Info = new PerfInfo(this);
            }

            // Starts disabled
            enabled = false;
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

        private class PerfInfoAdminGUI : AdminPanelGUI
        {
            private PerfInfoGUI _perfInfo;
            public PerfInfoAdminGUI(PerfInfoGUI perfInfo)
            {
                _perfInfo = perfInfo;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Performance Info");
                layout.CreateToggleButton("Show performance info", _perfInfo.enabled, (value) => {
                    _perfInfo.enabled = value; });
            }
        }
    }
}
