#if ADMIN_PANEL

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        static Text CreateItemText(GameObject go)
        {
            var text = go.AddComponent<Text>();
            text.font = DefaultFont;
            text.color = Color.white;
            text.fontSize = DefaultFontSize - 5;
            text.alignment = TextAnchor.MiddleLeft;
            return text;
        }

        static Color MofiyToggleColor(Color c)
        {
            return new Color(c.r, c.g, c.b, 1.0f);
        }

        static ColorBlock GetToggleColors()
        {
            var normal = MofiyToggleColor(BackgroundColor);
            var highlight = MofiyToggleColor(ForegroundColor);
            var colors = new ColorBlock();
            colors.normalColor = normal;
            colors.highlightedColor = highlight;
            colors.colorMultiplier = 1.05f;
            return colors;
        }

        public Dropdown CreateDropdown(string currentKey, string[] options, Action<string> onChange = null)
        {
            var rectTransform = CreateUIObject("Admin Panel - Dropdown", Parent);
            var dropdown = rectTransform.gameObject.AddComponent<Dropdown>();
            dropdown.gameObject.AddComponent<Image>().color = MofiyToggleColor(BackgroundColor);
            var label = CreateUIObject("Label", dropdown.transform);
            var template = CreateUIObject("Template", dropdown.transform);
            var item = CreateUIObject("Item", template);
            var itemLabel = CreateUIObject("Item Label", item);
            var toggle = item.gameObject.AddComponent<Toggle>();
            toggle.image = item.gameObject.AddComponent<Image>();
            toggle.colors = GetToggleColors();
            dropdown.template = template;
            dropdown.captionText = CreateItemText(label.gameObject);
            dropdown.itemText = CreateItemText(itemLabel.gameObject);
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultFontSize;
            layoutElement.flexibleWidth = 1;
            dropdown.value = 0;
            dropdown.captionText.text = currentKey;
            if(options != null)
            {
                dropdown.options = new List<Dropdown.OptionData>();
                dropdown.options.Add(new Dropdown.OptionData(currentKey));
                for(int i = 0, optionsLength = options.Length; i < optionsLength; i++)
                {
                    var option = options[i];
                    dropdown.options.Add(new Dropdown.OptionData(option));
                }
            }
            if(onChange != null)
            {
                dropdown.onValueChanged.AddListener(pos => {
                    if(options != null && pos > 0 && pos <= options.Length)
                    {
                        onChange(options[pos - 1]);
                    }
                    else
                    {
                        onChange(null);
                    }
                });
            }

            return dropdown;
        }
    }
}

#endif
