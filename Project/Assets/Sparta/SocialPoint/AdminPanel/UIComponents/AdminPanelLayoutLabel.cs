#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public Text CreateLabel(string label)
        {
            var rectTransform = CreateUIObject("Admin Panel - Label", Parent);

            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = label;
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperCenter;

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            return text;
        }

        public Text CreateFormLabel(string label)
        {
            var text = CreateLabel(label);
            var layoutElement = text.GetComponent<LayoutElement>();
            layoutElement.flexibleWidth = 0;
            layoutElement.minWidth = 150;
            return text;
        }
    }
}

#endif
