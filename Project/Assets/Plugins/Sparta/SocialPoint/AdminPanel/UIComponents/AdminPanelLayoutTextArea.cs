#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public Text CreateTextArea(string content = null)
        {
            // If the string is long enough it doesnt show anything in the test box.
            // I havent fully tested the limit, but 15000 is long enough
            if(content != null && content.Length > 15000)
            {
                content = content.Substring(0, 15000);
            }
            
            // Console
            var rectTransform = CreateUIObject("Admin Panel - Text Area", Parent);
            rectTransform.gameObject.AddComponent<LayoutElement>();

            var textComponent = rectTransform.gameObject.AddComponent<Text>();
            textComponent.text = content ?? string.Empty;
            textComponent.font = DefaultFont;
            textComponent.fontSize = TextAreaFontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.UpperLeft;

            ContentSizeFitter fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return textComponent;
        }
    }
}

#endif
