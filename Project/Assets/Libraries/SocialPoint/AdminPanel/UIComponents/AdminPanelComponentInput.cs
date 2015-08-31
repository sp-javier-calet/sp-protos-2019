using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public void CreateTextInput(string placeholder, Action<string> onSubmit, Action<string> onValueChange)
        {
            var rectTransform = CreateUIObject("Admin Panel - Text Input", Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = InputColor;
            
            var input = rectTransform.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;

            if(onSubmit != null)
            {
                input.onEndEdit.AddListener((value) => {
                    if(value.Length > 0)
                    {
                        onSubmit(value);
                    }
                });
            }

            if(onValueChange != null)
            {
                input.onValueChange.AddListener((value) => {
                    onValueChange(value); 
                });
            }

            // Placeholder
            var contentTransform = CreateUIObject("Admin Panel - Text Input Placeholder", rectTransform);
            contentTransform.anchoredPosition = new Vector2(DefaultMargin, -DefaultMargin);

            var text = contentTransform.gameObject.AddComponent<Text>();
            text.text = placeholder;
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize;
            text.color = Color.gray;
            text.alignment = TextAnchor.UpperLeft;

            input.placeholder = text;

            // Suggestion
            /*contentTransform = CreateUIObject("Admin Panel - Text Input Suggestion", rectTransform);
            contentTransform.anchoredPosition = new Vector2(DefaultMargin, -DefaultMargin);
            
            text = contentTransform.gameObject.AddComponent<Text>();
            text.text = "Suggested text";
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize;
            text.color = new Color(.7f, .7f, .7f, .7f);
            text.alignment = TextAnchor.UpperLeft;
            */

            // Content
            contentTransform = CreateUIObject("Admin Panel - Text Input content", rectTransform);
            contentTransform.anchoredPosition = new Vector2(DefaultMargin, -DefaultMargin);

            text = contentTransform.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize;
            text.color = Color.gray;
            text.alignment = TextAnchor.UpperLeft;

            input.textComponent = text;
        }

        public void CreateTextInput(string placeholder, Action<string> onSubmit)
        {
            CreateTextInput(placeholder, onSubmit, null);
        }
    }
}
