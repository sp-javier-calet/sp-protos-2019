using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public Text CreateTextArea(string content = null)
        {
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

