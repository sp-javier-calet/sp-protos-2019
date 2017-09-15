#if ADMIN_PANEL

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        Slider CreateSliderComponent(float current, string currentLabel, float min, float max, ButtonColor sliderColor, Action<Text, float> onChanged, bool enabled = true)
        {
            var rectTransform = CreateUIObject("Admin Panel - Slider", Parent);

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var slider = rectTransform.gameObject.AddComponent<Slider>();
            slider.enabled = enabled;

            // Background
            var sliderBackground = CreateUIObject("Admin Panel - Slider Background", rectTransform);
            sliderBackground.anchorMin = new Vector2(0.05f, 0.25f);
            sliderBackground.anchorMax = new Vector2(0.95f, 0.75f);
            var backgroundImage = sliderBackground.gameObject.AddComponent<Image>();
            backgroundImage.color = enabled ? sliderColor.Color : sliderColor.GetDisabled();

            // Fill
            var fillArea = CreateUIObject("Admin Panel - Slider Fill Area", rectTransform);
            fillArea.anchorMin = new Vector2(0.05f, 0.25f);
            fillArea.anchorMax = new Vector2(0.94f, 0.75f);

            var fill = CreateUIObject("Admin Panel - Slider Fill", fillArea);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.zero;
            fill.sizeDelta = new Vector2(10, 0);
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = enabled ? sliderColor.Color : sliderColor.GetDisabled();

            // Value Label
            var label = CreateUIObject("Admin Panel - Slider Label", rectTransform);
            label.anchorMin = new Vector2(0.06f, 0.1f);
            label.anchorMax = new Vector2(0.94f, 0.9f);
            var labelText = label.gameObject.AddComponent<Text>();
            labelText.alignment = TextAnchor.MiddleRight;
            labelText.color = Color.white;
            labelText.font = DefaultFont;
            labelText.text = currentLabel;

            // Handler
            var handlerArea = CreateUIObject("Admin Panel - Slider Handler Area", rectTransform);
            handlerArea.anchorMin = new Vector2(0.05f, 0.1f);
            handlerArea.anchorMax = new Vector2(0.94f, 0.9f);

            var handler = CreateUIObject("Admin Panel - Slider Handler", handlerArea);
            handler.sizeDelta = new Vector2(10.0f, 0);
            var handlerImage = handler.gameObject.AddComponent<Image>();
            handlerImage.color = enabled ? ForegroundColor : DisabledColor;

            // Configure Slider
            slider.targetGraphic = handlerImage;
            slider.fillRect = fill;
            slider.handleRect = handler;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = current;
            slider.onValueChanged.AddListener(value => onChanged(labelText, value));

            return slider;
        }

        public Slider CreateSlider(float current, float min, float max, Action<float> onChanged, bool enabled = true)
        {
            return CreateSlider(current, min, max, ButtonColor.Default, onChanged, enabled);
        }

        public Slider CreateSlider(float current, float min, float max, ButtonColor sliderColor, Action<float> onChanged, bool enabled = true)
        {
            return CreateSliderComponent(current, current.ToString(), min, max, sliderColor, (label, value) => {
                label.text = value.ToString();
                onChanged(value);
            }, enabled);
        }

        public Slider CreateSliderDiscrete(int current, int min, int max, Action<int> onChanged, bool enabled = true)
        {
            return CreateSliderDiscrete(current, min, max, ButtonColor.Default, onChanged, enabled);
        }

        public Slider CreateSliderDiscrete(int current, int min, int max, ButtonColor sliderColor, Action<int> onChanged, bool enabled = true)
        {
            var slider = CreateSliderComponent((float)current, current.ToString(), (float)min, (float)max, sliderColor,
                (label, value) => {
                    label.text = value.ToString();
                    var intValue = (int)value;
                    onChanged(intValue);
                }, enabled);

            slider.wholeNumbers = true;
            return slider;
        }

        public Slider CreateSlider<T>(T current, IList<T> list, Action<T> onChanged, bool enabled = true)
        {
            return CreateSlider(current, list, ButtonColor.Default, onChanged, enabled);
        }

        public Slider CreateSlider<T>(T current, IList<T> list, ButtonColor sliderColor, Action<T> onChanged, bool enabled = true)
        {
            var index = list.IndexOf(current);
            if(index < 0)
            {
                index = 0;
            }
            var currentElm = list[index];

            var slider = CreateSliderComponent((float)index, currentElm.ToString(), 0, list.Count - 1, sliderColor, 
                (label, value) => {
                    var elm = list[(int)value];
                    label.text = elm.ToString();
                    onChanged(elm);
                }, enabled);

            slider.wholeNumbers = true;
            return slider;
        }
    }
}

#endif
