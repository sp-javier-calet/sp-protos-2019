using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public void CreateTextArea(string content, out Text text)
        {
            // Console
            var rectTransform = CreateUIObject("Admin Panel - Text Area", Parent);
            rectTransform.gameObject.AddComponent<LayoutElement>();

            var textComponent = rectTransform.gameObject.AddComponent<Text>();
            textComponent.text = content;
            textComponent.font = DefaultFont;
            textComponent.fontSize = TextAreaFontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.UpperLeft;

            text = textComponent;
            
            ContentSizeFitter fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;;
        }

        public void CreateTextArea(string content)
        {
            Text text;
            CreateTextArea(content, out text);
        }
    }
}

