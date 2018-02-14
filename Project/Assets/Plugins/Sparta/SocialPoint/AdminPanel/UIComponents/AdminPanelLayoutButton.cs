#if ADMIN_PANEL

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using SocialPoint.AssetBundlesClient;
using SocialPoint.Base;
using SocialPoint.Dependency;

namespace SocialPoint.AdminPanel
{
    public sealed class ButtonColor
    {
        public Color Color { get; private set; }

        const float Alpha = 0.7f;

        ButtonColor(float r, float g, float b)
        {
            Color = new Color(r, g, b, Alpha);
        }

        public Color GetDisabled()
        {
            return new Color(Color.r * 0.5f, Color.g * 0.5f, Color.b * 0.5f, Alpha);
        }

        public Color GetHighlight()
        {
            return new Color(Color.r * 1.2f, Color.g * 1.2f, Color.b * 1.2f, Alpha);
        }

        public static readonly ButtonColor Default = new ButtonColor(.5f, .5f, .5f);
        public static readonly ButtonColor Disabled = new ButtonColor(.2f, .2f, .2f);
        public static readonly ButtonColor Gray = new ButtonColor(.5f, .5f, .5f);
        public static readonly ButtonColor Red = new ButtonColor(.8f, .5f, .5f);
        public static readonly ButtonColor Green = new ButtonColor(.5f, .8f, .5f);
        public static readonly ButtonColor Blue = new ButtonColor(.5f, .5f, .8f);
        public static readonly ButtonColor Yellow = new ButtonColor(.8f, .8f, .5f);
    }

    public partial class AdminPanelLayout
    {
        public delegate void Callback(AssetBundleLoadAssetOperation request);
        /*
         * Generic Button
         */

        public Button CreateButton(string label, Action onClick, bool enabled = true, bool bestFit = false)
        {
            return CreateButton(label, ButtonColor.Default, onClick, enabled, null, bestFit);
        }

        public Button CreateButtonWithIcon(string label, Action onClick, string imagePath, bool bestFit, bool enabled = true)
        {
            return CreateButton(label, ButtonColor.Default, onClick, enabled, imagePath, bestFit);
        }

        public Button CreateButtonWithIcon(string label, ButtonColor buttonColor, Action onClick, string imagePath, bool bestFit, bool enabled = true)
        {
            return CreateButton(label, buttonColor, onClick, enabled, imagePath, bestFit);
        }

        public Button CreateButton(string label, ButtonColor buttonColor, Action onClick, bool enabled = true, string imagePath = null, bool bestFit = false)
        {
            var rectTransform = CreateUIObject("Admin Panel - Button", Parent);

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = enabled ? buttonColor.Color : ButtonColor.Disabled.Color;
            var button = rectTransform.gameObject.AddComponent<Button>();
            CreateButtonLabel(label, rectTransform, FontStyle.Normal, enabled, bestFit, (imagePath == null) ? 1 : 0.75f);
            if(enabled && onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            Callback cb = (AssetBundleLoadAssetOperation request) =>
            {
                if(request != null)
                {
                    var texture = request.GetAsset<Texture>();
                    if(texture != null)
                    {
                        var textureTransform = CreateUIObject("Admin Panel - Icon Button", rectTransform);

                        Vector2 anchorMin = textureTransform.anchorMin;
                        Vector2 anchorMax = textureTransform.anchorMax;
                        anchorMin.x = 0.75f;
                        anchorMax.x = 1f;

                        textureTransform.anchorMin = anchorMin;
                        textureTransform.anchorMax = anchorMax;
                        textureTransform.pivot = new Vector2(0.5f, 0.5f);
                        
                        var iconImage = textureTransform.gameObject;

                        Image uitexture = iconImage.AddComponent<Image>();
                        uitexture.preserveAspect = true;
                        uitexture.sprite = Sprite.Create(texture as Texture2D, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        uitexture.raycastTarget = false;
                        Color col = uitexture.color;
                        col.a = 0.7f;
                        uitexture.color = col;
                    }
                }

                button.targetGraphic = image;
            };

            if(imagePath != null)
            {
                Services.Instance.Resolve<AssetBundleManager>().CoroutineRunner.StartCoroutine(SetImageIcon(imagePath, cb));
            }

            return button;
        }

        void LoadAsync(string path, RawImage image)
        {
            image.texture = Resources.Load(path) as Texture;
        }

        IEnumerator SetImageIcon(string imagePath, Callback callBack)
        {
            AssetBundleLoadAssetOperation request = null;

            // Get the asset
            yield return Services.Instance.Resolve<AssetBundleManager>().LoadAssetAsyncRequest(imagePath + "_png", imagePath, typeof(Texture), req => request = req);

            // do something with it
            callBack(request);
        }

        /*
         * Confirm Button
         */

        public ConfirmActionButton CreateConfirmButton(string label, Action onClick, bool enabled = true)
        {
            return CreateConfirmButton(label, ButtonColor.Default, onClick, enabled);
        }

        public ConfirmActionButton CreateConfirmButton(string label, ButtonColor buttonColor, Action onClick, bool enabled = true)
        {
            var rectTransform = CreateUIObject("Admin Panel - Confirm Button", Parent);

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = enabled ? buttonColor.Color : ButtonColor.Disabled.Color;

            var confirm = rectTransform.gameObject.AddComponent<ConfirmActionButton>();
            confirm.ButtonImage = image;

            if(enabled)
            {
                confirm.onSubmit = onClick;
            }

            rectTransform.gameObject.AddComponent<EventTrigger>();

            CreateButtonLabel(label, rectTransform, FontStyle.BoldAndItalic, enabled);

            return confirm;
        }

        public sealed class ConfirmActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            const float TimeToCompletion = 1.0f;
            const float TimeToDisabled = 0.2f;

            public Action onSubmit;
            public Image ButtonImage;

            Color _initialColor;
            bool _pressed;
            float _completion;

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

                        UpdateButtonColor();

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

            void UpdateButtonColor()
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
        public Button CreateOpenPanelButton(string label, IAdminPanelGUI panel, bool enabled = true, bool replacePanel = false)
        {
            return CreateOpenPanelButton(label, ButtonColor.Default, panel, enabled, replacePanel);
        }

        public Button CreateOpenPanelButton(string label, ButtonColor buttonColor, IAdminPanelGUI panel, bool enabled = true, bool replacePanel = false)
        {
            var rectTransform = CreateUIObject("Admin Panel - Open Panel Button", Parent);

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = enabled ? buttonColor.Color : ButtonColor.Disabled.Color;

            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = button.colors.pressedColor;
            button.colors = colors;

            if(enabled)
            {
                button.onClick.AddListener(() =>
                {
                    if(replacePanel)
                    {
                        ReplacePanel(panel);
                    }
                    else
                    {
                        OpenPanel(panel);
                    }
                });
            }

            CreateButtonLabel(label, rectTransform, FontStyle.Normal, enabled);
            CreateOpenPanelIndicator(rectTransform);

            EventSystem.current.SetSelectedGameObject(rectTransform.gameObject);

            return button;
        }

        /*
         * Toggle Button
         */

        public Toggle CreateToggleButton(string label, bool status, Action<bool> onToggle, bool enabled = true)
        {
            return CreateToggleButton(label, status, ButtonColor.Default, onToggle, enabled);
        }

        public Toggle CreateToggleButton(string label, bool status, ButtonColor buttonColor, Action<bool> onToggle, bool enabled = true)
        {
            var rectTransform = CreateUIObject("Admin Panel - Toggle Button", Parent);

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var toggleBackground = CreateUIObject("Admin Panel - Toggle Background", rectTransform);
            var image = toggleBackground.gameObject.AddComponent<Image>();
            image.color = enabled ? buttonColor.Color : ButtonColor.Disabled.Color;

            // Status indicators parameters
            var anchorMin = Vector2.one;
            var anchorMax = Vector2.one;
            int indicatorSize = (int) Mathf.Round(DefaultLabelHeight / 3);
            var anchoredPosition = new Vector2(-indicatorSize * 2, -indicatorSize - 1);
            var indicatorSizeDelta = new Vector2(indicatorSize, indicatorSize);

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
            toggle.enabled = enabled;
            toggle.isOn = status;

            if(enabled)
            {
                toggle.onValueChanged.AddListener(value => onToggle(value));
            }

            CreateButtonLabel(label, rectTransform, FontStyle.Normal, enabled);

            return toggle;
        }

        /*
         * Foldout Button
         */

        Toggle CreateFoldoutButton(string label, bool status, Action<bool> onToggle, bool enabled = true)
        {
            return CreateFoldoutButton(label, status, ButtonColor.Default, onToggle, enabled);
        }

        Toggle CreateFoldoutButton(string label, bool status, ButtonColor buttonColor, Action<bool> onToggle, bool enabled = true)
        {
            var rectTransform = CreateUIObject("Admin Panel - Foldout Button", Parent);

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var toggleBackground = CreateUIObject("Admin Panel - Foldout Background", rectTransform);
            var image = toggleBackground.gameObject.AddComponent<Image>();
            image.color = enabled ? buttonColor.Color : ButtonColor.Disabled.Color;

            // Status indicators parameters
            var anchorMin = new Vector2(0, 0.04f);
            var anchorMax = new Vector2(1, 0.06f);
            var anchoredPosition = Vector2.zero;

            // Disabled indicator
            var disableIndicator = CreateUIObject("Admin Panel - Foldout Closed Graphic", rectTransform);
            disableIndicator.anchorMin = anchorMin;
            disableIndicator.anchorMax = anchorMax;
            disableIndicator.anchoredPosition = anchoredPosition;

            var disImage = disableIndicator.gameObject.AddComponent<Image>();
            disImage.color = Color.white;

            // Enabled indicator
            var toggleIndicator = CreateUIObject("Admin Panel - Foldout Opened Graphic", rectTransform);
            toggleIndicator.anchorMin = anchorMin;
            toggleIndicator.anchorMax = anchorMax;
            toggleIndicator.anchoredPosition = anchoredPosition;

            var indImage = toggleIndicator.gameObject.AddComponent<Image>();
            indImage.color = DisabledColor;

            // Toggle button
            var toggle = rectTransform.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = image;
            toggle.graphic = indImage;
            toggle.enabled = enabled;
            toggle.isOn = status;

            if(enabled)
            {
                toggle.onValueChanged.AddListener(value => onToggle(value));
            }

            CreateButtonLabel(label, rectTransform, FontStyle.Normal, enabled);

            return toggle;
        }

        /*
         * Internal
         */

        void CreateButtonLabel(string label, Transform buttonTransform, FontStyle style = FontStyle.Normal, bool enabled = true, bool bestFit = false, float xMaxAnchor = -1)
        {
            var rectTransform = CreateUIObject("Admin Panel - Button Label", buttonTransform);

            if(xMaxAnchor > 0)
            {
                Vector2 anchor = rectTransform.anchorMax;
                anchor.x = xMaxAnchor;
                rectTransform.anchorMax = anchor;
            }

            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = label;
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize;
            text.color = enabled ? Color.white : Color.gray;
            text.resizeTextForBestFit = bestFit;
            text.resizeTextMinSize = 10;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = style;

            LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
        }

        void CreateOpenPanelIndicator(Transform buttonTransform)
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

#endif