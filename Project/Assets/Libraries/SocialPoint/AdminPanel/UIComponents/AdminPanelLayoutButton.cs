using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public class ButtonColor
    {
        public Color Color { get; private set; }
        private static float Alpha = 0.7f;
        private ButtonColor(float r, float g, float b)
        {
            Color = new Color(r, g, b, Alpha);
        }
        
        public static readonly ButtonColor Default = new ButtonColor(.5f, .5f, .5f);
        public static readonly ButtonColor Gray = new ButtonColor(.5f, .5f, .5f);
        public static readonly ButtonColor Red = new ButtonColor(.8f, .5f, .5f);
        public static readonly ButtonColor Green = new ButtonColor(.5f, .8f, .5f);
        public static readonly ButtonColor Blue = new ButtonColor(.5f, .5f, .8f);
        public static readonly ButtonColor Yellow = new ButtonColor(.8f, .8f, .5f);
    }

    public partial class AdminPanelLayout
    {
        /*
         * Generic Button
         */

        public Button CreateButton(string label, Action onClick)
        {
            return CreateButton(label, ButtonColor.Default, onClick);
        }

        public Button CreateButton(string label, ButtonColor buttonColor, Action onClick)
        {
            var rectTransform = CreateUIObject("Admin Panel - Button", Parent);
          
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = buttonColor.Color;
            
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            if(onClick != null)
            {
                button.onClick.AddListener(() => {
                    onClick();
                });
            }
            
            CreateButtonLabel(label, rectTransform);

            return button;
        }


        /*
         * Confirm Button
         */

        public ConfirmActionButton CreateConfirmButton(string label, Action onClick)
        {
            return CreateConfirmButton(label, ButtonColor.Default, onClick);
        }

        public ConfirmActionButton CreateConfirmButton(string label, ButtonColor buttonColor, Action onClick)
        {
            var rectTransform = CreateUIObject("Admin Panel - Confirm Button", Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = buttonColor.Color;

            var confirm = rectTransform.gameObject.AddComponent<ConfirmActionButton>();
            confirm.ButtonImage = image;
            confirm.onSubmit = onClick;

            rectTransform.gameObject.AddComponent<EventTrigger>();
            
            CreateButtonLabel(label, rectTransform, FontStyle.BoldAndItalic);

            return confirm;
        }

        public class ConfirmActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            private static float TimeToCompletion = 1.0f;
            private static float TimeToDisabled = 0.2f;

            public Action onSubmit;
            public Image ButtonImage;

            private Color _initialColor;
            private bool _pressed;
            private float _completion;

            void Start()
            {
                _initialColor = ButtonImage.color;
                _completion = 0;
            }

            void Update()
            {
                if(_pressed)
                {
                    _completion += Time.deltaTime / TimeToCompletion;

                    if(_completion >= 1.0f)
                    {
                        _completion = 0f;
                        _pressed = false;

                        if(onSubmit != null)
                        {
                            onSubmit();
                        }
                    }

                    UpdateButtonColor();
                }
                else if(_completion > 0)
                {
                    _completion -= Time.deltaTime / TimeToDisabled;
                    UpdateButtonColor();
                }
            }

            private void UpdateButtonColor()
            {
                if(ButtonImage != null)
                {
                    ButtonImage.color = Color.Lerp(_initialColor, Color.white, _completion);
                }
            }

            public void OnPointerDown(PointerEventData data)
            {
                _pressed = true;
            }

            public void OnPointerUp(PointerEventData data)
            {
                _pressed = false;
            }
        }


        /*
         * Open Panel Button
         */

        public Button CreateOpenPanelButton(string label, IAdminPanelGUI panel, ButtonColor buttonColor)
        {
            return CreateOpenPanelButton(label, panel, false, ButtonColor.Default);
        }

        public Button CreateOpenPanelButton(string label, IAdminPanelGUI panel, bool replacePanel = false)
        {
            return CreateOpenPanelButton(label, panel, replacePanel, ButtonColor.Default);
        }

        public Button CreateOpenPanelButton(string label, IAdminPanelGUI panel, bool replacePanel, ButtonColor buttonColor)
        {
            var rectTransform = CreateUIObject("Admin Panel - Open Panel Button", Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = buttonColor.Color;
            
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => { 
                if(replacePanel)
                {
                    ReplacePanel(panel);
                }
                else
                {
                    OpenPanel(panel);
                }
            });
            
            CreateButtonLabel(label, rectTransform);
            CreateOpenPanelIndicator(rectTransform);

            return button;
        }



        /*
         * Toggle Button
         */

        public Toggle CreateToggleButton(string label, bool status, Action<bool> onToggle)
        {
            return CreateToggleButton(label, status, ButtonColor.Default, onToggle);
        }

        public Toggle CreateToggleButton(string label, bool status, ButtonColor buttonColor, Action<bool> onToggle)
        {
            var rectTransform = CreateUIObject("Admin Panel - Toggle Button", Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
            
            var toggleBackground = CreateUIObject("Admin Panel - Toggle Background", rectTransform);
            var image = toggleBackground.gameObject.AddComponent<Image>();
            image.color = buttonColor.Color;
            
            // Status indicators parameters
            Vector2 anchorMin = Vector2.one;
            Vector2 anchorMax = Vector2.one;
            int indicatorSize = (int)Mathf.Round(DefaultLabelHeight / 3);
            Vector2 anchoredPosition = new Vector2(-indicatorSize * 2, -indicatorSize - 1);
            Vector2 indicatorSizeDelta = new Vector2(indicatorSize, indicatorSize);
            
            // Disabled indicator
            var disableIndicator = CreateUIObject("Admin Panel - Toggle Disabled Graphic", rectTransform);
            disableIndicator.anchorMin = anchorMin;
            disableIndicator.anchorMax = anchorMax;
            disableIndicator.anchoredPosition = anchoredPosition;
            disableIndicator.sizeDelta = indicatorSizeDelta;
            
            var disImage = disableIndicator.gameObject.AddComponent<Image>();
            disImage.color = StatusDisabledColor;
            
            // Enabled indicator
            var toggleIndicator = CreateUIObject("Admin Panel - Toggle Enabled Graphic", rectTransform);
            toggleIndicator.anchorMin = anchorMin;
            toggleIndicator.anchorMax = anchorMax;
            toggleIndicator.anchoredPosition = anchoredPosition;
            toggleIndicator.sizeDelta = indicatorSizeDelta;
            
            var indImage = toggleIndicator.gameObject.AddComponent<Image>();
            indImage.color = StatusEnabledColor;
            
            // Toggle button
            var toggle = rectTransform.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = image;
            toggle.graphic = indImage;
            
            toggle.isOn = status;
            toggle.onValueChanged.AddListener((value) => {
                onToggle(value); 
            });
            
            CreateButtonLabel(label, rectTransform);

            return toggle;
        }

        /*
         * Internal
         */

        private void CreateButtonLabel(string label, RectTransform buttonTransform, FontStyle style = FontStyle.Normal)
        {
            var rectTransform = CreateUIObject("Admin Panel - Button Label", buttonTransform);
            
            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = label;
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = style;
            
            LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
        }

        private void CreateOpenPanelIndicator(RectTransform buttonTransform)
        {
            var rectTransform = CreateUIObject("Admin Panel - Open Panel Indicator", buttonTransform);
            rectTransform.anchorMin = Vector2.right;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = new Vector2(-DefaultFontSize * 1.5f, 0);
            rectTransform.sizeDelta = new Vector2(DefaultFontSize, 1.0f);

            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = ">";
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize / 2;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleRight;
            
            LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
        }
    }
}
