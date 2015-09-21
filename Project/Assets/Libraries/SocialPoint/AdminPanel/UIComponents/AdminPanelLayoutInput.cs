using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public InputField CreateTextInput(string placeholder, Action<string> onSubmit, Action<InputStatus> onValueChange)
        {
            var rectTransform = CreateUIObject("Admin Panel - Text Input", Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = InputColor;
            
            var input = rectTransform.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;

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
            contentTransform = CreateUIObject("Admin Panel - Text Input Suggestion", rectTransform);
            contentTransform.anchoredPosition = new Vector2(DefaultMargin, -DefaultMargin);
            
            var suggestionText = contentTransform.gameObject.AddComponent<Text>();
            suggestionText.text = "";
            suggestionText.font = DefaultFont;
            suggestionText.fontSize = DefaultFontSize;
            suggestionText.color = new Color(.7f, .7f, .7f, .7f);
            suggestionText.alignment = TextAnchor.UpperLeft;

            // Content
            contentTransform = CreateUIObject("Admin Panel - Text Input content", rectTransform);
            contentTransform.anchoredPosition = new Vector2(DefaultMargin, -DefaultMargin);

            var contentText = contentTransform.gameObject.AddComponent<Text>();
            contentText.font = DefaultFont;
            contentText.fontSize = DefaultFontSize;
            contentText.color = Color.gray;
            contentText.alignment = TextAnchor.UpperLeft;


            // Configure input
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
                    InputStatus status = new InputStatus(value, suggestionText.text);

                    onValueChange(status); 

                    contentText.text = status.Content;
                    suggestionText.text = status.Suggestion;
                });
            }

            input.textComponent = contentText;
            return input;
        }

        public InputField CreateTextInput(string placeholder, Action<string> onSubmit)
        {
            return CreateTextInput(placeholder, onSubmit, null);
        }

        public class InputStatus
        {
            public string Suggestion { get; set; }
            public string Content { get; set; }
            
            public InputStatus(string content, string suggestion)
            {
                Content = content;
                Suggestion = suggestion;
            }
        }
    }
}
