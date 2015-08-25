using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;


namespace SocialPoint.AdminPanel
{
    public class AdminPanelGUIUtils  
    {
        private const int DefaultFontSize = 20;
        private const int DefaultButtonHeight = 25;
        private const int DefaultMargin = 5;


        private static float CreateButtonObject(AdminPanelLayout layout, string label, UnityAction onClickAction)
        {
            var buttonObject = new GameObject("AdminPanel - Button");

            var image = buttonObject.AddComponent<Image>();
            buttonObject.transform.SetParent(layout.Parent);
            image.rectTransform.anchoredPosition = layout.Position;

            image.rectTransform.anchorMin = new Vector2(0, 0.5f);
            image.rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
            image.rectTransform.sizeDelta = new Vector2(1.0f, DefaultButtonHeight);
            image.color = new Color(1f, .3f, .3f, .5f);
            
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClickAction);

            using(AdminPanelLayout buttonLayout = new AdminPanelLayout(buttonObject.transform))
            {
                AdminPanelGUIUtils.CreateLabelObject(buttonLayout, label);
            }

            return DefaultButtonHeight + DefaultMargin;
        }

        private static float CreateLabelObject(AdminPanelLayout layout, string label)
        {
            var textObject = new GameObject("Admin Panel - Label");
            textObject.transform.SetParent(layout.Parent);
            var text = textObject.AddComponent<Text>();
            text.rectTransform.anchoredPosition = layout.Position;
            text.rectTransform.sizeDelta = new Vector2(1.0f, 25.0f);
            text.rectTransform.anchorMin = new Vector2(0.0f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
            text.text = label;
            text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            return text.preferredHeight;
        }

        public static void CreateButton(AdminPanelLayout layout, string label, UnityAction onClickAction)
        {
            layout.Advance(AdminPanelGUIUtils.CreateButtonObject(layout, label, onClickAction));
        }
    
        public static void CreateLabel(AdminPanelLayout layout, string label)
        {
            layout.Advance(AdminPanelGUIUtils.CreateLabelObject(layout, label));
        }

        public static void CreateMargin(AdminPanelLayout layout, float marginMultiplier = 2.0f)
        {
            layout.Advance(DefaultMargin * marginMultiplier);
        }
    }
}
